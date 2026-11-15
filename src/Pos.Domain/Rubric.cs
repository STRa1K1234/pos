namespace Pos.Domain;

public sealed class Rubric
{
    private readonly List<Criterion> _criteria = [];
    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();
    internal Rubric(IEnumerable<Criterion> criteria)
    {
        foreach (var criterion in criteria) Add(criterion);
    }
    public void Add(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        if (_criteria.Any(c => c.Id == criterion.Id || string.Equals(c.Name, criterion.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Критерий с таким номером или названием уже существует.");
        if (_criteria.Sum(c => c.Weight) + criterion.Weight > 1)
            throw new DomainException("Сумма весов не должна превышать 1.");
        _criteria.Add(criterion);
    }
    public Criterion Get(Guid id) => _criteria.SingleOrDefault(c => c.Id == id)
        ?? throw new DomainException("Критерий отсутствует в рубрикаторе.");
    public void EnsureComplete()
    {
        if (_criteria.Count == 0 || _criteria.Sum(c => c.Weight) != 1)
            throw new DomainException("Для расчёта итога сумма весов должна равняться 1.");
    }
}
