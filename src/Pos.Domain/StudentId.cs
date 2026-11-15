namespace Pos.Domain;

public sealed record StudentId
{
    public string Value { get; }
    public StudentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException("Номер студента не может быть пустым.");
        Value = value.Trim();
    }
    public override string ToString() => Value;
}
