using System.Diagnostics;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetRunner;

public class ProcessExecution : IProcessExecution
{
    private readonly ILogger Log;
    internal string Output = string.Empty;
    internal string Error = string.Empty;
    
    public ProcessExecution(ILogger logger)
    {
        Log = logger;
    }
    
    public Task<Process?> Start(ProcessStartInfo processStartInfo)
    {
        //reset variables for the case of rerun
        Output = string.Empty;
        Error = string.Empty;

        return Task.Run(() => Process.Start(processStartInfo));
    }

    public void FetchOutput(Process process)
    {
        process.OutputDataReceived += (sender, args) =>
        {
            Log.Verbose(args.Data!);
            Output += $"\n{args.Data}";
        };
        process.BeginOutputReadLine();
    }

    public void FetchError(Process process)
    {
        process.ErrorDataReceived += (sender, args) =>
        {
            Log.Error(args.Data!);
            Error += $"\n{args.Data}";
        };
        process.BeginErrorReadLine();
    }

    public async Task<int> End(Process process)
    {
        int waitForExitSeconds = 5;
        while(!process.HasExited)
        {
            Log.Debug($"Process still running... Waiting for {waitForExitSeconds} seconds before checking again.");
            await Task.Delay(TimeSpan.FromSeconds(waitForExitSeconds));
        }

        Log.Debug($"Process has exited with code {process.ExitCode}");
        return process.ExitCode;
    }

    public string GetOutput()
        => Output;

    public string GetError()
        => Error;
}