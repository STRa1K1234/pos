namespace Pos.Domain;

public sealed class RubricBuilder
{
    private readonly List<Criterion> _criteria = [];
    public RubricBuilder Add(string name, decimal maximum, decimal weight)
    {
        _criteria.Add(new Criterion(Guid.NewGuid(), name, maximum, weight));
        return this;
    }
    public Rubric Build()
    {
        var rubric = new Rubric(_criteria);
        rubric.EnsureComplete();
        return rubric;
    }
    public static Rubric Empty() => new([]);
}
