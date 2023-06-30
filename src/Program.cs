using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Reflection;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetTestRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    .AddSingleton<ILogger, Logger>()
    .AddSingleton<RerunCommand>()
    .AddSingleton<RerunCommandConfiguration>()
    .AddSingleton<IDotNetTestRunner, DotNetTestRunner>()
    .AddSingleton<IProcessExecution, ProcessExecution>()
    .AddSingleton<ITestResultsAnalyzer, TestResultsAnalyzer>()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();

var Log = serviceProvider.GetRequiredService<ILogger>();
var cmd = serviceProvider.GetService<RerunCommand>();

await new CommandLineBuilder(cmd)
    .UseDefaults()
    .UseExceptionHandler((exception, context) =>
    {
        if (exception is RerunException
            || (exception is TargetInvocationException
            && exception.InnerException is RerunException))
        {
            Log.Error(exception.Message);
            Log.Debug(exception.StackTrace ?? string.Empty);
        }
        else
        {
            Log.Exception(exception);
        }
        
        Environment.ExitCode = 1;
    })
    .Build()
    .InvokeAsync(args);

return Environment.ExitCode;