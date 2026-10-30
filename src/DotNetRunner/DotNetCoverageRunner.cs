using System.Diagnostics;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetCoverageRunner : IDotNetCoverageRunner
{
    private int ExitCode;
    private readonly ILogger Log;
    private string CommandLine = string.Empty;
    private readonly IProcessExecution ProcessExecution;
    private string[] SearchPatterns = { "*.coverage", "*.cobertura*" }; 

    public DotNetCoverageRunner(ILogger logger,
        IProcessExecution processExecution)
    {
        Log = logger;
        ProcessExecution = processExecution;
    }

    /// <summary>
    /// Merges the coverage reports produced since <paramref name="startDate"/>
    /// </summary>
    /// <param name="options">The test-rerun options.</param>
    /// <param name="resultsDirectory">The results directory.</param>
    /// <param name="startDate">Initial date to look for files.</param>
    /// <param name="cancellationToken">Cancels the merge and stops the dotnet-coverage process.</param>
    public async Task Merge(RerunOptions options, string resultsDirectory, DateTime startDate, CancellationToken cancellationToken = default)
    {
        var arguments = CoverageMergeArguments.Build(options, GetCoverageFiles(resultsDirectory, startDate), resultsDirectory);
        await Run(arguments, cancellationToken);
    }

    /// <summary>
    /// Runs dotnet command with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task Run(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet-coverage",
            WorkingDirectory = "/",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in arguments)
            processStartInfo.ArgumentList.Add(argument);
        CommandLine = CommandLineDisplay.Render(processStartInfo.FileName, arguments);

        Log.Debug($"working directory: {processStartInfo.WorkingDirectory}");
        Log.Debug($"forking {CommandLine}");
        string unexpectedError = string.Empty;
        try
        {
            using Process? ps = await ProcessExecution.Start(processStartInfo);
            ProcessExecution.FetchOutput(ps!);
            ProcessExecution.FetchError(ps!);
            ExitCode = await ProcessExecution.End(ps!, cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            ExitCode = 1;
            unexpectedError = e.Message;
        }
        HandleProcessEnd(unexpectedError);
    }

    /// <summary>
    /// Handles the process end.
    /// </summary>
    /// <exception cref="RerunException">dotnet-coverage exited with a non-zero code</exception>
    private void HandleProcessEnd(string unexpectedError)
    {
        if (ExitCode != 0)
        {
            Log.Verbose(GetErrorMessage(unexpectedError));
            Log.Verbose($"Exit code {ExitCode}.");
            throw new RerunException($"command failed:\n{CommandLine} \n" +
                                     $"reason: {GetErrorMessage(unexpectedError)}");
        }
    }

    private IEnumerable<string> GetCoverageFiles(string resultsDirectory, DateTime startDate)
        => SearchPatterns.SelectMany(pattern =>
                        Directory.GetFiles(resultsDirectory, pattern, SearchOption.AllDirectories))
                        .Select(file => new FileInfo(file))
                        .Where(fileInfo => fileInfo.CreationTime >= startDate)
                        .Select(fileInfo => fileInfo.FullName);
    
    private string GetErrorMessage(string unexpectedError)
        => string.IsNullOrWhiteSpace(unexpectedError) ? ProcessExecution.GetError() : unexpectedError;
}