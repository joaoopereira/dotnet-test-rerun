using System.Diagnostics;
using System.IO.Abstractions;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetTestRunner : IDotNetTestRunner
{
    private static readonly string[] WellKnownErrors = { "No test source files were specified." };
    private readonly ILogger Log;
    private readonly IProcessRunner ProcessRunner;
    private readonly IFileSystem FileSystem;

    public DotNetTestRunner(ILogger logger,
        IProcessRunner processRunner,
        IFileSystem fileSystem)
    {
        Log = logger;
        ProcessRunner = processRunner;
        FileSystem = fileSystem;
    }

    /// <summary>
    /// Runs dotnet test and classifies how it ended.
    /// </summary>
    /// <param name="options">The test-rerun options.</param>
    /// <param name="request">The filter, framework and results directory of this run.</param>
    /// <param name="cancellationToken">Cancels the run and stops the dotnet test process.</param>
    /// <exception cref="RerunException">dotnet test failed for a reason other than failed tests</exception>
    public async Task<TestRunResult> Test(RerunOptions options, RunRequest request, CancellationToken cancellationToken = default)
    {
        var arguments = DotNetTestArguments.Build(options, request);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Directory.GetCurrentDirectory()
        };
        foreach (var argument in arguments)
            processStartInfo.ArgumentList.Add(argument);
        var commandLine = CommandLineDisplay.Render(processStartInfo.FileName, arguments);

        Log.Debug($"working directory: {processStartInfo.WorkingDirectory}");
        Log.Debug($"forking {commandLine}");

        var trxFilesBeforeRun = GetTrxFiles(request.ResultsDirectory);
        var result = await ProcessRunner.RunAsync(processStartInfo, cancellationToken);
        var outcome = Classify(result, HaveProducedTrxFiles(request.ResultsDirectory, trxFilesBeforeRun), commandLine);
        return new TestRunResult(outcome, result.ExitCode);
    }

    /// <summary>
    /// Classifies how dotnet test ended. Structured signals come first: a non-zero exit code
    /// with TRX files produced by this run means tests failed (or the run was aborted), which the
    /// TRX analysis handles. The console text is only a fallback, as it is localised.
    /// </summary>
    private ErrorCode Classify(ProcessResult result, bool haveProducedTrxFiles, string commandLine)
    {
        if (result.ExitCode == 0)
            return ErrorCode.Success;

        if (haveProducedTrxFiles)
            return ErrorCode.FailedTests;

        if (IsWellKnownError(result.Error))
        {
            Log.Warning(result.Error);
            return ErrorCode.WellKnownError;
        }

        if (HaveFailedTestsInOutput(result.Output))
        {
            Log.Debug("No TRX file was produced; detected failed tests from the dotnet test output.");
            return ErrorCode.FailedTests;
        }

        Log.Verbose(result.Error);
        Log.Verbose($"Exit code {result.ExitCode}.");
        throw new RerunException($"command: {commandLine} exited with code {result.ExitCode}. \n\nError message: {result.Error}");
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

    private static bool IsWellKnownError(string error)
        => WellKnownErrors.Any(error.Contains);

    /// <summary>
    /// Fallback for runs without TRX output: English dotnet test summary or test host crash messages.
    /// </summary>
    private static bool HaveFailedTestsInOutput(string output)
        => output.Contains("Failed:") ||
           output.Contains("Test host process crashed") ||
           output.Contains("Test Run Aborted");
}
