namespace Pos.Domain;

public sealed record FinalScore(decimal Value, int EvaluatedCount, int CriterionCount, string Policy);

public interface IFinalScorePolicy
{
    string Name { get; }
    decimal Calculate(Rubric rubric, GradeBook book);
}

// Пропуски учитываются как нули, знаменатель не меняется.
public sealed class MissingAsZeroPolicy : IFinalScorePolicy
{
    public string Name => "Пропуски = 0";
    public decimal Calculate(Rubric rubric, GradeBook book) => rubric.Criteria.Sum(c =>
        (book.Evaluations.SingleOrDefault(e => e.CriterionId == c.Id)?.Score ?? 0) / c.MaxScore * c.Weight) * 10;
}

public sealed class RequireAllPolicy : IFinalScorePolicy
{
    public string Name => "Все оценки обязательны";
    public decimal Calculate(Rubric rubric, GradeBook book)
    {
        if (rubric.Criteria.Any(c => book.Evaluations.All(e => e.CriterionId != c.Id)))
            throw new DomainException("Итог недоступен: выставлены не все оценки.");
        return new MissingAsZeroPolicy().Calculate(rubric, book);
    }
}

public sealed class FinalScoreService
{
    public FinalScore Calculate(Rubric rubric, GradeBook book, IFinalScorePolicy policy)
    {
        rubric.EnsureComplete();
        foreach (var evaluation in book.Evaluations)
        {
            var criterion = rubric.Get(evaluation.CriterionId);
            if (evaluation.Score < 0 || evaluation.Score > criterion.MaxScore)
                throw new DomainException("Оценка не согласована с рубрикатором.");
        }
        return new FinalScore(decimal.Round(policy.Calculate(rubric, book), 2, MidpointRounding.AwayFromZero),
            book.Evaluations.Count, rubric.Criteria.Count, policy.Name);
    }
}
