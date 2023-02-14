using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Reflection;
using dotnet.test.rerun;
using dotnet.test.rerun.Logging;
using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    .AddSingleton<ILogger, Logger>()
    .AddSingleton<RerunCommand>()
    .AddSingleton<RerunCommandConfiguration>()
    .AddSingleton<dotnet.test.rerun.dotnet>()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();

var Log = serviceProvider.GetRequiredService<ILogger>();
var cmd = serviceProvider.GetService<RerunCommand>();

return await new CommandLineBuilder(cmd)
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
    })
    .Build()
    .InvokeAsync(args);