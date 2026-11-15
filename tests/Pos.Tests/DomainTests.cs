using Pos.Domain;
namespace Pos.Tests;
public class DomainTests
{
    private static Rubric Rubric() => new RubricBuilder().Add("ДЗ", 20, .4m).Add("Экзамен", 10, .6m).Build();
    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void ScoreOutsideRangeIsRejected(int score)
    {
        var rubric = Rubric(); var book = new GradeBook(new StudentId("1"));
        Assert.Throws<DomainException>(() => book.Set(rubric, rubric.Criteria[0].Id, score));
        Assert.Empty(book.Evaluations);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    public void BoundaryScoresAreAccepted(int score)
    {
        var rubric = Rubric(); var book = new GradeBook(new StudentId("1"));
        book.Set(rubric, rubric.Criteria[0].Id, score);
        Assert.Equal(score, Assert.Single(book.Evaluations).Score);
    }
    [Fact]
    public void UnknownCriterionIsRejected()
    {
        var book = new GradeBook(new StudentId("1"));
        Assert.Throws<DomainException>(() => book.Set(Rubric(), Guid.NewGuid(), 1));
    }
    [Fact]
    public void CorrectionAndCancellationChangeCalculatedResult()
    {
        var rubric = Rubric(); var book = new GradeBook(new StudentId("1"));
        var calculator = new FinalScoreService(); var policy = new MissingAsZeroPolicy();
        book.Set(rubric, rubric.Criteria[0].Id, 10);
        Assert.Equal(2m, calculator.Calculate(rubric, book, policy).Value);
        book.Set(rubric, rubric.Criteria[0].Id, 20);
        Assert.Single(book.Evaluations);
        Assert.Equal(4m, calculator.Calculate(rubric, book, policy).Value);
        Assert.Throws<DomainException>(() => book.Set(rubric, rubric.Criteria[0].Id, 21));
        Assert.Equal(20m, Assert.Single(book.Evaluations).Score);
        book.Cancel(rubric.Criteria[0].Id);
        Assert.Equal(0m, calculator.Calculate(rubric, book, policy).Value);
    }
    [Fact]
    public void WeightedResultNormalizesDifferentMaximums()
    {
        var rubric = Rubric(); var book = new GradeBook(new StudentId("1"));
        book.Set(rubric, rubric.Criteria[0].Id, 16); book.Set(rubric, rubric.Criteria[1].Id, 8);
        Assert.Equal(8m, new FinalScoreService().Calculate(rubric, book, new RequireAllPolicy()).Value);
    }
    [Fact]
    public void StrictPolicyRejectsMissingGrades() => Assert.Throws<DomainException>(() =>
        new FinalScoreService().Calculate(Rubric(), new GradeBook(new StudentId("1")), new RequireAllPolicy()));
    [Fact]
    public void IncompleteRubricCannotProduceResult() => Assert.Throws<DomainException>(() =>
        new FinalScoreService().Calculate(RubricBuilder.Empty(), new GradeBook(new StudentId("1")), new MissingAsZeroPolicy()));
    [Fact] public void BuilderRejectsIncompleteRubric() => Assert.Throws<DomainException>(() => new RubricBuilder().Add("ДЗ", 10, .5m).Build());
    [Fact]
    public void ExcessWeightDoesNotModifyRubric()
    {
        var rubric = Rubric();
        Assert.Throws<DomainException>(() => rubric.Add(new Criterion(Guid.NewGuid(), "Ещё", 10, .1m)));
        Assert.Equal(2, rubric.Criteria.Count);
    }
    [Fact]
    public void DuplicateNameIsRejected() => Assert.Throws<DomainException>(() =>
        new RubricBuilder().Add("ДЗ", 10, .5m).Add("дз", 10, .5m).Build());
    [Theory]
    [InlineData(0, .5)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    [InlineData(10, 2)]
    public void InvalidCriterionIsRejected(double maximum, double weight) => Assert.Throws<DomainException>(() =>
        new Criterion(Guid.NewGuid(), "ДЗ", (decimal)maximum, (decimal)weight));
    [Fact] public void StudentIdHasValueEquality() => Assert.Equal(new StudentId(" 1 "), new StudentId("1"));
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyStudentIdIsRejected(string id) => Assert.Throws<DomainException>(() => new StudentId(id));
    [Fact]
    public void ForeignRubricIsRejected()
    {
        var rubric = Rubric(); var book = new GradeBook(new StudentId("1"));
        book.Set(rubric, rubric.Criteria[0].Id, 10);
        Assert.Throws<DomainException>(() => new FinalScoreService().Calculate(Rubric(), book, new MissingAsZeroPolicy()));
    }
}
