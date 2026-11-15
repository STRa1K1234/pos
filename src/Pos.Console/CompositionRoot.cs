using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
namespace Pos.ConsoleApp;

public static class CompositionRoot
{
    public static ServiceProvider Build() => new ServiceCollection()
        .AddSingleton<IStudentRepository, InMemoryStudentRepository>()
        .AddSingleton<IGradeBookRepository, InMemoryGradeBookRepository>()
        .AddSingleton<IRubricRepository, InMemoryRubricRepository>()
        .AddSingleton<IResultExporter, CsvResultExporter>()
        .AddSingleton<IFinalScorePolicy, MissingAsZeroPolicy>()
        .AddSingleton<IFinalScorePolicy, RequireAllPolicy>()
        .AddSingleton<FinalScoreService>()
        .AddSingleton<PosFacade>()
        .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
