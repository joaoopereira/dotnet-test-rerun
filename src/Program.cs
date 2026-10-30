using System.CommandLine;
using System.IO.Abstractions;
using dotnet.test.rerun;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.Cli;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;
using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    .AddSingleton<ILogger, Logger>()
    .AddSingleton<RerunRootCommand>()
    .AddSingleton<RerunCliDefinition>()
    .AddSingleton<RerunOrchestrator>()
    .AddSingleton<IDotNetTestRunner, DotNetTestRunner>()
    .AddSingleton<IDotNetCoverageRunner, DotNetCoverageRunner>()
    .AddSingleton<IProcessRunner, ProcessRunner>()
    .AddSingleton<ITestResultsAnalyzer, TestResultsAnalyzer>()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();

var log = serviceProvider.GetRequiredService<ILogger>();
var command = serviceProvider.GetRequiredService<RerunRootCommand>();
int exitCode;

try
{
    var parseResult = command.Parse(args);
    // Let exceptions reach the handlers below instead of System.CommandLine's default handler,
    // so RerunException is reported as a plain message and cancellation gets its own exit code.
    exitCode = await parseResult.InvokeAsync(new InvocationConfiguration { EnableDefaultExceptionHandler = false });
}
catch (OperationCanceledException)
{
    log.Warning("Test run cancelled.");
    exitCode = 130;
}
catch (RerunException exception)
{
    log.Error(exception.Message);
    log.Debug(exception.StackTrace ?? string.Empty);
    exitCode = 1;
}
catch (Exception exception)
{
    log.Exception(exception);
    exitCode = 1;
}

Environment.ExitCode = exitCode;
return exitCode;
