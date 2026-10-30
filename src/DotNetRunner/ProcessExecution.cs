using System.Diagnostics;
using System.Text;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetRunner;

public class ProcessExecution : IProcessExecution
{
    private readonly ILogger Log;
    internal StringBuilder Output = new();
    internal StringBuilder Error = new();
    
    public ProcessExecution(ILogger logger)
    {
        Log = logger;
    }
    
    public Task<Process?> Start(ProcessStartInfo processStartInfo)
    {
        //reset variables for the case of rerun
        Output = new StringBuilder();
        Error = new StringBuilder();

        return Task.Run(() => Process.Start(processStartInfo));
    }

    public void FetchOutput(Process process)
    {
        var output = Output;
        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data is null)
                return;

            Log.Verbose(args.Data);
            lock (output)
                output.Append('\n').Append(args.Data);
        };
        process.BeginOutputReadLine();
    }

    public void FetchError(Process process)
    {
        var error = Error;
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data is null)
                return;

            Log.Error(args.Data);
            lock (error)
                error.Append('\n').Append(args.Data);
        };
        process.BeginErrorReadLine();
    }

    /// <summary>
    /// Waits for the process to exit and for its redirected output to be fully read.
    /// On cancellation the whole process tree is killed and the cancellation is rethrown.
    /// </summary>
    public async Task<int> End(Process process, CancellationToken cancellationToken = default)
    {
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("Cancellation requested. Stopping the running process...");
            KillProcessTree(process);
            throw;
        }

        Log.Debug($"Process has exited with code {process.ExitCode}");
        return process.ExitCode;
    }

    public string GetOutput()
    {
        lock (Output)
            return Output.ToString();
    }

    public string GetError()
    {
        lock (Error)
            return Error.ToString();
    }

    private void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            Log.Debug($"Could not kill process: {e.Message}");
        }
    }
}
