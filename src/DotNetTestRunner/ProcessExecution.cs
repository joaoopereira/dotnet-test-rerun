using System.Diagnostics;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetTestRunner;

public class ProcessExecution : IProcessExecution
{
    private readonly ILogger Log;
    internal string Output;
    internal string Error;
    internal int ExitCode;
    
    public ProcessExecution(ILogger logger)
    {
        Log = logger;
    }
    
    public Task<Process?> Start(ProcessStartInfo processStartInfo)
        => Task.Run(() => Process.Start(processStartInfo));

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
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    public string GetOutput()
        => Output;

    public string GetError()
        => Error;
}