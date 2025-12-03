using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Reflection;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    .AddSingleton<ILogger, Logger>()
    .AddSingleton<RerunCommand>()
    .AddSingleton<RerunCommandConfiguration>()
    .AddSingleton<IDotNetTestRunner, DotNetTestRunner>()
    .AddSingleton<IDotNetCoverageRunner, DotNetCoverageRunner>()
    .AddSingleton<IProcessExecution, ProcessExecution>()
    .AddSingleton<ITestResultsAnalyzer, TestResultsAnalyzer>()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();

var Log = serviceProvider.GetRequiredService<ILogger>();
var cmd = serviceProvider.GetService<RerunCommand>();

// Parse and invoke with custom exception handling
try
{
    var parseResult = cmd!.Parse(args);
    var result = await parseResult.InvokeAsync();
    Environment.ExitCode = result;
}
catch (Exception exception)
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
}

return Environment.ExitCode;