using System.Diagnostics;
using System.Text;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetRunner;

public class ProcessRunner : IProcessRunner
{
    private readonly ILogger Log;

    public ProcessRunner(ILogger logger)
    {
        Log = logger;
    }

    public async Task<ProcessResult> RunAsync(ProcessStartInfo processStartInfo, CancellationToken cancellationToken = default)
    {
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;

        var output = new StringBuilder();
        var error = new StringBuilder();

        using var process = new Process { StartInfo = processStartInfo };
        process.OutputDataReceived += (_, args) => Append(output, args.Data, Log.Verbose);
        process.ErrorDataReceived += (_, args) => Append(error, args.Data, Log.Error);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            // also waits until the redirected output has been fully read
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("Cancellation requested. Stopping the running process...");
            KillProcessTree(process);
            throw;
        }

        Log.Debug($"Process has exited with code {process.ExitCode}");
        return new ProcessResult(process.ExitCode, Read(output), Read(error));
    }

    private static void Append(StringBuilder builder, string? line, Action<string> log)
    {
        if (line is null)
            return;

        log(line);
        lock (builder)
            builder.Append('\n').Append(line);
    }

    private static string Read(StringBuilder builder)
    {
        lock (builder)
            return builder.ToString();
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
