using Pos.Domain;
namespace Pos.Application;

public sealed class PosFacade(
    IStudentRepository students, IGradeBookRepository books, IRubricRepository rubrics,
    FinalScoreService calculator, IEnumerable<IFinalScorePolicy> policies, IResultExporter exporter)
{
    private readonly IReadOnlyList<IFinalScorePolicy> _policies = policies.ToList();
    public IReadOnlyList<string> PolicyNames => _policies.Select(p => p.Name).ToList();
    public IReadOnlyList<Student> Students => students.List();
    public IReadOnlyList<Criterion> Criteria => rubrics.Get().Criteria;
    public void AddStudent(string id, string name) => students.Add(new Student(new StudentId(id), name));
    public void AddCriterion(string name, decimal maximum, decimal weight) =>
        rubrics.Get().Add(new Criterion(Guid.NewGuid(), name, maximum, weight));
    public void SetGrade(string studentId, Guid criterionId, decimal score, string comment = "")
    {
        var student = students.Get(new StudentId(studentId));
        books.Get(student.Id).Set(rubrics.Get(), criterionId, score, comment);
    }
    public void CancelGrade(string studentId, Guid criterionId)
    {
        var student = students.Get(new StudentId(studentId));
        books.Get(student.Id).Cancel(criterionId);
    }
    public IReadOnlyCollection<Evaluation> Grades(string studentId) => books.Get(students.Get(new StudentId(studentId)).Id).Evaluations;
    public FinalScore Result(string studentId, int policyIndex) => calculator.Calculate(rubrics.Get(),
        books.Get(students.Get(new StudentId(studentId)).Id), Policy(policyIndex));
    public void Export(string path, int policyIndex)
    {
        // Сначала рассчитываем все строки: ошибка не оставит частичный отчёт.
        var rows = Students.Select(s =>
        {
            var result = Result(s.Id.Value, policyIndex);
            return new ResultRow(s.Id.Value, s.Name, result.Value, result.EvaluatedCount, result.CriterionCount, result.Policy);
        }).ToList();
        exporter.Export(rows, path);
    }
    private IFinalScorePolicy Policy(int index) => index >= 0 && index < _policies.Count
        ? _policies[index] : throw new DomainException("Неизвестное правило расчёта.");
}
