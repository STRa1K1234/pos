namespace Pos.Domain;

public sealed class Criterion
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }
    public Criterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        if (id == Guid.Empty) throw new DomainException("Пустой идентификатор критерия.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Название критерия не может быть пустым.");
        if (maxScore <= 0) throw new DomainException("Максимум должен быть положительным.");
        if (weight <= 0 || weight > 1) throw new DomainException("Вес должен быть в диапазоне (0; 1].");
        Id = id; Name = name.Trim(); MaxScore = maxScore; Weight = weight;
    }
}
