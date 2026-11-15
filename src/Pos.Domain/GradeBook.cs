namespace Pos.Domain;

public sealed class GradeBook
{
    private readonly Dictionary<Guid, Evaluation> _evaluations = [];
    public StudentId StudentId { get; }
    public IReadOnlyCollection<Evaluation> Evaluations => _evaluations.Values.ToList().AsReadOnly();
    public GradeBook(StudentId studentId) => StudentId = studentId ?? throw new ArgumentNullException(nameof(studentId));
    public void Set(Rubric rubric, Guid criterionId, decimal score, string comment = "")
    {
        var evaluation = new Evaluation(rubric.Get(criterionId), score, comment);
        _evaluations[criterionId] = evaluation;
    }
    public void Cancel(Guid criterionId)
    {
        if (!_evaluations.Remove(criterionId)) throw new DomainException("Оценка не найдена.");
    }
}
