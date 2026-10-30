using System.Diagnostics;
using System.IO.Abstractions;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetTestRunner : IDotNetTestRunner
{
    private ErrorCode ErrorCode;
    private int ExitCode;
    private readonly ILogger Log;
    private string CommandLine = string.Empty;
    private readonly IProcessExecution ProcessExecution;
    private readonly IFileSystem FileSystem;
    private string[] WellKnownErrors = { "No test source files were specified." };

    public DotNetTestRunner(ILogger logger,
        IProcessExecution processExecution,
        IFileSystem fileSystem)
    {
        Log = logger;
        ProcessExecution = processExecution;
        FileSystem = fileSystem;
    }

    /// <summary>
    /// Runs dotnet test with the specified arguments
    /// </summary>
    /// <param name="options">The test-rerun options.</param>
    /// <param name="request">The filter, framework and results directory of this run.</param>
    /// <param name="cancellationToken">Cancels the run and stops the dotnet test process.</param>
    public async Task Test(RerunOptions options, RunRequest request, CancellationToken cancellationToken = default)
    {
        var trxFilesBeforeRun = GetTrxFiles(request.ResultsDirectory);
        await Run(DotNetTestArguments.Build(options, request), cancellationToken);
        HandleProcessEnd(HaveProducedTrxFiles(request.ResultsDirectory, trxFilesBeforeRun));
    }

    public ErrorCode GetErrorCode()
        => ErrorCode;

    /// <summary>
    /// Runs dotnet test with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task Run(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in arguments)
            processStartInfo.ArgumentList.Add(argument);
        CommandLine = CommandLineDisplay.Render(processStartInfo.FileName, arguments);

        Log.Debug($"working directory: {processStartInfo.WorkingDirectory}");
        Log.Debug($"forking {CommandLine}");

        using Process? ps = await ProcessExecution.Start(processStartInfo);
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
            throw new RerunException($"command: {CommandLine} exited with code {ExitCode}. \n\nError message: {ProcessExecution.GetError()}");
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
