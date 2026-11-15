using Pos.Domain;
namespace Pos.Application;

public interface IStudentRepository
{
    IReadOnlyList<Student> List();
    Student Get(StudentId id);
    void Add(Student student);
}
public interface IGradeBookRepository
{
    GradeBook Get(StudentId id);
}
public interface IRubricRepository
{
    Rubric Get();
}
public sealed record ResultRow(string StudentId, string Name, decimal Score, int Evaluated, int Total, string Policy);
public interface IResultExporter
{
    void Export(IReadOnlyList<ResultRow> rows, string path);
}
