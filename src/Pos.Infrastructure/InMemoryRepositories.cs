using Pos.Application;
using Pos.Domain;
namespace Pos.Infrastructure;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<StudentId, Student> _students = [];
    public IReadOnlyList<Student> List() => _students.Values.OrderBy(s => s.Id.Value, StringComparer.Ordinal).ToList().AsReadOnly();
    public Student Get(StudentId id) => _students.GetValueOrDefault(id) ?? throw new DomainException("Студент не найден.");
    public void Add(Student student)
    {
        if (!_students.TryAdd(student.Id, student)) throw new DomainException("Студент с таким номером уже существует.");
    }
}
public sealed class InMemoryGradeBookRepository : IGradeBookRepository
{
    private readonly Dictionary<StudentId, GradeBook> _books = [];
    public GradeBook Get(StudentId id)
    {
        if (!_books.TryGetValue(id, out var book)) _books.Add(id, book = new GradeBook(id));
        return book;
    }
}
public sealed class InMemoryRubricRepository : IRubricRepository
{
    private readonly Rubric _rubric = RubricBuilder.Empty();
    public Rubric Get() => _rubric;
}
