using System.Diagnostics;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetCoverageRunner : IDotNetCoverageRunner
{
    private int ExitCode;
    private readonly ILogger Log;
    private readonly ProcessStartInfo ProcessStartInfo;
    private readonly IProcessExecution ProcessExecution;
    private string[] SearchPatterns = { "*.coverage", "*.cobertura*" }; 

    public DotNetCoverageRunner(ILogger logger,
        IProcessExecution processExecution)
    {
        ProcessStartInfo = new()
        {
            FileName = "dotnet-coverage",
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
    /// <param name="startDate">Initial date to look for files.</param>
    public async Task Merge(RerunCommandConfiguration config, string resultsDirectory, DateTime startDate)
    {
        string arguments = config.GetMergeCoverageArgumentList(GetCoverageFiles(resultsDirectory, startDate), resultsDirectory);

        if (string.IsNullOrEmpty(resultsDirectory) is false)
            arguments = $"{arguments} --results-directory {resultsDirectory}";

        await Run(arguments);
    }

    /// <summary>
    /// Runs dotnet command with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    private async Task Run(string arguments)
    {
        Log.Debug($"working directory: {ProcessStartInfo.WorkingDirectory}");
        Log.Debug($"forking {arguments}");
        ProcessStartInfo.Arguments = arguments;
        string unexpectedError = string.Empty;
        try
        {
            using Process? ps = await ProcessExecution.Start(ProcessStartInfo);
            ProcessExecution.FetchOutput(ps!);
            ProcessExecution.FetchError(ps!);
            ExitCode = await ProcessExecution.End(ps!);
        }
        catch (Exception e)
        {
            ExitCode = 1;
            unexpectedError = e.Message;
        }
        HandleProcessEnd(unexpectedError);
    }

    /// <summary>
    /// Handles the process end.
    /// </summary>
    /// <exception cref="RerunException">command:\n\n\t\tdotnet {ProcessStartInfo.Arguments}</exception>
    private void HandleProcessEnd(string unexpectedError)
    {
        if (ExitCode != 0)
        {
            Log.Verbose(GetErrorMessage(unexpectedError));
            Log.Verbose($"Exit code {ExitCode}.");
            throw new RerunException($"command failed:\ndotnet {ProcessStartInfo.Arguments} \n" +
                                     $"reason: {GetErrorMessage(unexpectedError)}");
        }
    }

    private string GetCoverageFiles(string resultsDirectory, DateTime startDate)
        => string.Join(" ", SearchPatterns.SelectMany(pattern =>
                        Directory.GetFiles(resultsDirectory, pattern, SearchOption.AllDirectories))
                        .Select(file => new FileInfo(file))
                        .Where(fileInfo => fileInfo.CreationTime >= startDate)
                        .Select(fileInfo => fileInfo.FullName));
    
    private string GetErrorMessage(string unexpectedError)
        => string.IsNullOrWhiteSpace(unexpectedError) ? ProcessExecution.GetError() : unexpectedError;
}