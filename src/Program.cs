using System.IO.Abstractions;
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

var exitCode = await serviceProvider.GetRequiredService<RerunRootCommand>().RunAsync(args);

Environment.ExitCode = exitCode;
return exitCode;
