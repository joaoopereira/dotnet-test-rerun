using System.Diagnostics;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetTestRunner : IDotNetTestRunner
{
    private ErrorCode ErrorCode;
    private int ExitCode;
    private readonly ILogger Log;
    private readonly ProcessStartInfo ProcessStartInfo;
    private readonly IProcessExecution ProcessExecution;
    private string[] WellKnownErrors = { "No test source files were specified." };

    public DotNetTestRunner(ILogger logger,
        IProcessExecution processExecution)
    {
        ProcessStartInfo = new()
        {
            FileName = "dotnet",
            WorkingDirectory = "/",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        Log = logger;
        ProcessExecution = processExecution;
    }

    /// <summary>
    /// Runs dotnet test with the specified arguments
    /// </summary>
    /// <param name="config">The config.</param>
    /// <param name="resultsDirectory">The results directory.</param>
    public async Task Test(RerunCommandConfiguration config, string resultsDirectory)
        => await Run(config.GetTestArgumentList(resultsDirectory));

    public ErrorCode GetErrorCode()
        => ErrorCode;

    /// <summary>
    /// Runs dotnet test with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    private async Task Run(string arguments)
    {
        Log.Debug($"working directory: {ProcessStartInfo.WorkingDirectory}");
        Log.Debug($"forking {arguments}");
        ProcessStartInfo.Arguments = arguments;

        using Process? ps = await ProcessExecution.Start(ProcessStartInfo);
        ProcessExecution.FetchOutput(ps!);
        ProcessExecution.FetchError(ps!);
        ExitCode = await ProcessExecution.End(ps!);

        HandleProcessEnd();
    }

    /// <summary>
    /// Handles the process end.
    /// </summary>
    /// <exception cref="RerunException">command:\n\n\t\tdotnet {ProcessStartInfo.Arguments}</exception>
    private void HandleProcessEnd()
    {
        if (ExitCode != 0)
        {
            if (IsWellKnownError())
            {
                ErrorCode = ErrorCode.WellKnownError;
                Log.Warning(ProcessExecution.GetError());
            }
            else if (HaveFailedTests())
            {
                ErrorCode = ErrorCode.FailedTests;
            }
            else
            {
                ErrorCode = ErrorCode.Error;
                Log.Verbose(ProcessExecution.GetError());
                Log.Verbose($"Exit code {ExitCode}.");
                throw new RerunException($"command:\ndotnet {ProcessStartInfo.Arguments}");
            }
        }
        else
        {
            ErrorCode = ErrorCode.Success;
        }
    }

    /// <summary>
    /// Determines whether [is well known error] [the specified exit code].
    /// </summary>
    /// <returns>
    ///   <c>true</c> if [is well known error] [the specified exit code]; otherwise, <c>false</c>.
    /// </returns>
    private bool IsWellKnownError() => ExitCode == 1 && WellKnownErrors.Contains(ProcessExecution.GetError());

    /// <summary>
    /// Check if the output of dotnet test have in the last line failed tests
    /// </summary>
    /// <returns></returns>
    private bool HaveFailedTests() => ExitCode == 1 &&
                                      (ProcessExecution.GetOutput().Contains("Failed!  - Failed:") &&
                                       ProcessExecution.GetOutput().Split("\n")[^2].StartsWith("Failed!  - Failed:"));
}