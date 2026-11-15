namespace Pos.Domain;

public sealed record Evaluation
{
    public Guid CriterionId { get; }
    public decimal Score { get; }
    public string Comment { get; }
    internal Evaluation(Criterion criterion, decimal score, string comment)
    {
        if (score < 0 || score > criterion.MaxScore)
            throw new DomainException($"Балл должен быть от 0 до {criterion.MaxScore}.");
        CriterionId = criterion.Id; Score = score; Comment = comment.Trim();
    }
}
