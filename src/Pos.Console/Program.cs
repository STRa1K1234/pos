using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.ConsoleApp;
using Pos.Domain;
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
using var provider = CompositionRoot.Build();
var app = provider.GetRequiredService<PosFacade>();
try
{
    if (args.Length > 0)
    {
        if (args[0] != "--demo" || args.Length > 2) throw new ArgumentException("Запуск: --demo [путь.csv]");
        Demo(app, args.Length == 2 ? args[1] : "exports/demo.csv");
    }
    else Menu(app);
}
catch (EndOfStreamException) { Console.WriteLine("\nВвод закончился."); }
catch (Exception e) when (e is DomainException or ArgumentException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Ошибка: {e.Message}");
    Environment.ExitCode = 1;
}

static void Menu(PosFacade app)
{
    Console.WriteLine("ПОС — преподавательская оценка студентов. Данные хранятся до выхода.");
    while (true)
    {
        Console.WriteLine("\n1 Студент: добавить | 2 Студенты: список | 3 Критерий: добавить | 4 Рубрикатор");
        Console.WriteLine("5 Оценка: выставить/исправить | 6 Оценки студента | 7 Оценка: отменить");
        Console.WriteLine("8 Итог | 9 Экспорт итогов CSV | 0 Выход");
        var choice = Read("Пункт");
        try
        {
            switch (choice)
            {
                case "0": return;
                case "1": app.AddStudent(Read("Номер студента"), Read("ФИО")); break;
                case "2":
                    foreach (var s in app.Students) Console.WriteLine($"{s.Id}: {s.Name}");
                    if (app.Students.Count == 0) Console.WriteLine("Студентов пока нет.");
                    break;
                case "3": app.AddCriterion(Read("Название"), Number("Максимум"), Number("Вес (0..1)")); break;
                case "4": PrintCriteria(app); break;
                case "5":
                    var student = Read("Номер студента");
                    var criterion = ChooseCriterion(app);
                    app.SetGrade(student, criterion, Number("Балл"), Read("Комментарий (можно пустой)", true));
                    break;
                case "6":
                    var grades = app.Grades(Read("Номер студента"));
                    foreach (var e in grades) Console.WriteLine($"{app.Criteria.Single(c => c.Id == e.CriterionId).Name}: {e.Score}; {e.Comment}");
                    if (grades.Count == 0) Console.WriteLine("Оценок пока нет.");
                    break;
                case "7": app.CancelGrade(Read("Номер студента"), ChooseCriterion(app)); break;
                case "8":
                    var id = Read("Номер студента");
                    var result = app.Result(id, ChoosePolicy(app));
                    Console.WriteLine($"Итог: {result.Value:F2}/10; оценено {result.EvaluatedCount}/{result.CriterionCount}; {result.Policy}");
                    break;
                case "9":
                    var path = Read("Путь CSV"); app.Export(path, ChoosePolicy(app));
                    Console.WriteLine($"Экспорт: {Path.GetFullPath(path)}"); break;
                default: Console.WriteLine("Выберите пункт от 0 до 9."); break;
            }
        }
        catch (Exception e) when (e is DomainException or ArgumentException or IOException or UnauthorizedAccessException)
        { Console.WriteLine($"Ошибка: {e.Message}"); }
    }
}
static string Read(string prompt, bool allowEmpty = false)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        var value = Console.ReadLine() ?? throw new EndOfStreamException();
        if (allowEmpty || !string.IsNullOrWhiteSpace(value)) return value.Trim();
        Console.WriteLine("Введите непустое значение.");
    }
}
static decimal Number(string prompt)
{
    while (true)
    {
        if (decimal.TryParse(Read(prompt).Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var value)) return value;
        Console.WriteLine("Нужно число, например 1.5 или 1,5.");
    }
}
static void PrintCriteria(PosFacade app)
{
    for (var i = 0; i < app.Criteria.Count; i++)
    {
        var c = app.Criteria[i]; Console.WriteLine($"{i + 1}: {c.Name}; максимум {c.MaxScore}; вес {c.Weight}");
    }
    Console.WriteLine($"Сумма весов: {app.Criteria.Sum(c => c.Weight)} (для итога нужна 1).");
}
static Guid ChooseCriterion(PosFacade app)
{
    if (app.Criteria.Count == 0) throw new DomainException("Сначала добавьте критерии.");
    PrintCriteria(app);
    var index = Number("Номер критерия");
    if (index != decimal.Truncate(index) || index < 1 || index > app.Criteria.Count) throw new DomainException("Неверный номер критерия.");
    return app.Criteria[(int)index - 1].Id;
}
static int ChoosePolicy(PosFacade app)
{
    for (var i = 0; i < app.PolicyNames.Count; i++) Console.WriteLine($"{i + 1}: {app.PolicyNames[i]}");
    var index = Number("Правило");
    if (index != decimal.Truncate(index) || index < 1 || index > app.PolicyNames.Count) throw new DomainException("Неверный номер правила.");
    return (int)index - 1;
}
static void Demo(PosFacade app, string path)
{
    app.AddStudent("255-01", "Анна Иванова"); app.AddStudent("255-02", "Борис Петров");
    var rubric = new RubricBuilder().Add("Домашние задания", 20, 0.4m).Add("Экзамен", 10, 0.6m).Build();
    foreach (var c in rubric.Criteria) app.AddCriterion(c.Name, c.MaxScore, c.Weight);
    Console.WriteLine("Созданы 2 студента и 2 критерия.");
    var hw = app.Criteria[0].Id; var exam = app.Criteria[1].Id;
    app.SetGrade("255-01", hw, 16); app.SetGrade("255-01", exam, 7);
    app.SetGrade("255-02", hw, 10);
    try { app.SetGrade("255-01", exam, 12); }
    catch (DomainException e) { Console.WriteLine($"Ожидаемый отказ: {e.Message}"); }
    try { app.SetGrade("255-01", Guid.NewGuid(), 5); }
    catch (DomainException e) { Console.WriteLine($"Ожидаемый отказ: {e.Message}"); }
    app.SetGrade("255-01", exam, 8, "Исправление");
    app.CancelGrade("255-01", exam); app.SetGrade("255-01", exam, 8);
    foreach (var s in app.Students) Console.WriteLine($"{s.Name}: {app.Result(s.Id.Value, 0).Value:F2}/10");
    try { app.Result("255-02", 1); }
    catch (DomainException e) { Console.WriteLine($"Строгий итог: {e.Message}"); }
    app.Export(path, 0);
    Console.WriteLine($"CSV: {Path.GetFullPath(path)}");
}
