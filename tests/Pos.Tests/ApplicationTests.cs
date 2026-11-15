using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.ConsoleApp;
using Pos.Domain;
using Pos.Infrastructure;
namespace Pos.Tests;
public class ApplicationTests
{
    [Fact]
    public void EndToEndWithCorrectionCancellationAndExport()
    {
        using var services = CompositionRoot.Build(); var app = services.GetRequiredService<PosFacade>();
        app.AddStudent("1", "Иван, \"Иванов\""); app.AddCriterion("ДЗ", 20, .4m); app.AddCriterion("Экзамен", 10, .6m);
        app.SetGrade("1", app.Criteria[0].Id, 16); app.SetGrade("1", app.Criteria[1].Id, 8);
        Assert.Equal(8m, app.Result("1", 1).Value);
        app.CancelGrade("1", app.Criteria[1].Id);
        Assert.Equal(3.2m, app.Result("1", 0).Value);
        Assert.Throws<DomainException>(() => app.Result("1", 1));
        app.SetGrade("1", app.Criteria[1].Id, 10);
        Assert.Equal(9.2m, app.Result("1", 1).Value);
        var path = Path.GetTempFileName();
        try
        {
            app.Export(path, 1);
            var csv = File.ReadAllText(path);
            Assert.Contains("9.2", csv); Assert.Contains("\"Иван, \"\"Иванов\"\"\"", csv);
            Assert.Equal(2, File.ReadAllLines(path).Length);
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public void UnknownStudentCannotGetGrade()
    {
        using var services = CompositionRoot.Build(); var app = services.GetRequiredService<PosFacade>();
        app.AddCriterion("ДЗ", 10, 1);
        Assert.Throws<DomainException>(() => app.SetGrade("нет", app.Criteria[0].Id, 5));
    }
    [Fact]
    public void DuplicateStudentIsRejected()
    {
        var repository = new InMemoryStudentRepository(); repository.Add(new Student(new StudentId("1"), "Анна"));
        Assert.Throws<DomainException>(() => repository.Add(new Student(new StudentId("1"), "Борис")));
        Assert.Equal("Анна", Assert.Single(repository.List()).Name);
    }
    [Fact]
    public void ExportDoesNotOverwriteFileWhenCalculationFails()
    {
        using var services = CompositionRoot.Build(); var app = services.GetRequiredService<PosFacade>();
        app.AddStudent("1", "Анна"); app.AddCriterion("ДЗ", 10, 1);
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "original");
            Assert.Throws<DomainException>(() => app.Export(path, 1));
            Assert.Equal("original", File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public void FacadeCanUseFakeExporter()
    {
        var exporter = new FakeExporter();
        var app = new PosFacade(new InMemoryStudentRepository(), new InMemoryGradeBookRepository(),
            new InMemoryRubricRepository(), new FinalScoreService(), [new MissingAsZeroPolicy()], exporter);
        app.AddStudent("1", "Анна"); app.AddCriterion("ДЗ", 10, 1); app.SetGrade("1", app.Criteria[0].Id, 7);
        app.Export("unused.csv", 0);
        Assert.Equal(7m, Assert.Single(exporter.Rows).Score);
    }
    private sealed class FakeExporter : IResultExporter
    {
        public IReadOnlyList<ResultRow> Rows { get; private set; } = [];
        public void Export(IReadOnlyList<ResultRow> rows, string path) => Rows = rows;
    }
}
