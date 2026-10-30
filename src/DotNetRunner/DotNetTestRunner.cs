using System.Diagnostics;
using System.IO.Abstractions;
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
    private readonly IFileSystem FileSystem;
    private string[] WellKnownErrors = { "No test source files were specified." };

    public DotNetTestRunner(ILogger logger,
        IProcessExecution processExecution,
        IFileSystem fileSystem)
    {
        ProcessStartInfo = new()
        {
            FileName = "dotnet",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        Log = logger;
        ProcessExecution = processExecution;
        FileSystem = fileSystem;
    }

    /// <summary>
    /// Runs dotnet test with the specified arguments
    /// </summary>
    /// <param name="config">The config.</param>
    /// <param name="resultsDirectory">The results directory.</param>
    /// <param name="cancellationToken">Cancels the run and stops the dotnet test process.</param>
    public async Task Test(RerunCommandConfiguration config, string resultsDirectory, CancellationToken cancellationToken = default)
    {
        var trxFilesBeforeRun = GetTrxFiles(resultsDirectory);
        await Run(config.GetTestArgumentList(resultsDirectory), cancellationToken);
        HandleProcessEnd(HaveProducedTrxFiles(resultsDirectory, trxFilesBeforeRun));
    }

    public ErrorCode GetErrorCode()
        => ErrorCode;

    /// <summary>
    /// Runs dotnet test with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task Run(string arguments, CancellationToken cancellationToken)
    {
        Log.Debug($"working directory: {ProcessStartInfo.WorkingDirectory}");
        Log.Debug($"forking {arguments}");
        ProcessStartInfo.Arguments = arguments;

        using Process? ps = await ProcessExecution.Start(ProcessStartInfo);
        ProcessExecution.FetchOutput(ps!);
        ProcessExecution.FetchError(ps!);
        ExitCode = await ProcessExecution.End(ps!, cancellationToken);
    }

    /// <summary>
    /// Classifies how dotnet test ended. Structured signals come first: a non-zero exit code
    /// with TRX files produced by this run means tests failed (or the run was aborted), which the
    /// TRX analysis handles. The console text is only a fallback, as it is localised.
    /// </summary>
    /// <exception cref="RerunException">dotnet test failed for a reason other than failed tests</exception>
    private void HandleProcessEnd(bool haveProducedTrxFiles)
    {
        if (ExitCode == 0)
        {
            ErrorCode = ErrorCode.Success;
        }
        else if (haveProducedTrxFiles)
        {
            ErrorCode = ErrorCode.FailedTests;
        }
        else if (IsWellKnownError())
        {
            ErrorCode = ErrorCode.WellKnownError;
            Log.Warning(ProcessExecution.GetError());
        }
        else if (HaveFailedTestsInOutput())
        {
            Log.Debug("No TRX file was produced; detected failed tests from the dotnet test output.");
            ErrorCode = ErrorCode.FailedTests;
        }
        else
        {
            ErrorCode = ErrorCode.Error;
            Log.Verbose(ProcessExecution.GetError());
            Log.Verbose($"Exit code {ExitCode}.");
            throw new RerunException($"command: dotnet {ProcessStartInfo.Arguments} exited with code {ExitCode}. \n\nError message: {ProcessExecution.GetError()}");
        }
    }

    /// <summary>
    /// Compares the TRX files before and after the run instead of using timestamps,
    /// as file system timestamps can lag behind the system clock.
    /// </summary>
    private bool HaveProducedTrxFiles(string resultsDirectory, IReadOnlyDictionary<string, DateTime> trxFilesBeforeRun)
        => GetTrxFiles(resultsDirectory).Any(file =>
            !trxFilesBeforeRun.TryGetValue(file.Key, out var lastWriteTime) || lastWriteTime != file.Value);

    private IReadOnlyDictionary<string, DateTime> GetTrxFiles(string resultsDirectory)
    {
        if (string.IsNullOrWhiteSpace(resultsDirectory))
            return new Dictionary<string, DateTime>();

        var directory = FileSystem.DirectoryInfo.New(resultsDirectory);
        return directory.Exists
            ? directory.EnumerateFiles("*.trx").ToDictionary(file => file.FullName, file => file.LastWriteTimeUtc)
            : new Dictionary<string, DateTime>();
    }

    private bool IsWellKnownError()
    {
        var error = ProcessExecution.GetError();
        return WellKnownErrors.Any(wellKnownError => error.Contains(wellKnownError));
    }

    /// <summary>
    /// Fallback for runs without TRX output: English dotnet test summary or test host crash messages.
    /// </summary>
    private bool HaveFailedTestsInOutput()
    {
        var output = ProcessExecution.GetOutput();
        return output.Contains("Failed:") ||
               output.Contains("Test host process crashed") ||
               output.Contains("Test Run Aborted");
    }
}
