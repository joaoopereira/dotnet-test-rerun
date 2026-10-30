using System.Diagnostics;

namespace dotnet.test.rerun.DotNetRunner;

public sealed record ProcessResult(int ExitCode, string Output, string Error);

public interface IProcessRunner
{
    /// <summary>
    /// Runs the process to completion, streaming its output to the log, and returns what it printed.
    /// On cancellation the whole process tree is killed and the cancellation is rethrown.
    /// </summary>
    Task<ProcessResult> RunAsync(ProcessStartInfo processStartInfo, CancellationToken cancellationToken = default);
}
