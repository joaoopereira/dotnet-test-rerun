using System.CommandLine;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace dotnet.test.rerun;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Setup Dependency Injection
        var serviceProvider = new ServiceCollection()
            .AddSingleton<Logger>()
            .AddSingleton<RerunCommand>()
            .AddSingleton<RerunCommandConfiguration>()
            .AddSingleton<dotnet>()
            .AddSingleton<IFileSystem>(new FileSystem())
            .BuildServiceProvider();

        var Log = serviceProvider.GetRequiredService<Logger>();

        try
        {
            var runnerCommand = serviceProvider.GetRequiredService<RerunCommand>();

            // Run
            return await runnerCommand.InvokeAsync(args);
        }
        catch (RerunException e)
        {
            Log.Error(e.Message);
            Log.Debug(e.StackTrace);
        }
        catch (Exception e)
        {
            Log.Exception(e);
        }

        return -1;
    }
}