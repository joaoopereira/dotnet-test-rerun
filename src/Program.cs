using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using dotnet.test.rerun;
using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    .AddSingleton<Logger>()
    .AddSingleton<RerunCommand>()
    .AddSingleton<RerunCommandConfiguration>()
    .AddSingleton<dotnet.test.rerun.dotnet>()
    .AddSingleton<IFileSystem>(new FileSystem())
    .BuildServiceProvider();

var Log = serviceProvider.GetRequiredService<Logger>();
var cmd = serviceProvider.GetService<RerunCommand>();

try
{
    return await new CommandLineBuilder(cmd).Build().InvokeAsync(args);
}
catch (RerunException e)
{
    Log.Error(e.Message);
    Log.Debug(e.StackTrace ?? string.Empty);
}
catch (Exception e)
{
    Log.Exception(e);
}

return -1;