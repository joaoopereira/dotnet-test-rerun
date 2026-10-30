using System.Diagnostics;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetCoverageRunner : IDotNetCoverageRunner
{
    private static readonly string[] SearchPatterns = { "*.coverage", "*.cobertura*" };
    private readonly ILogger Log;
    private readonly IProcessRunner ProcessRunner;

    public DotNetCoverageRunner(ILogger logger,
        IProcessRunner processRunner)
    {
        Log = logger;
        ProcessRunner = processRunner;
    }

    /// <summary>
    /// Merges the coverage reports produced since <paramref name="startDate"/>
    /// </summary>
    /// <param name="options">The test-rerun options.</param>
    /// <param name="resultsDirectory">The results directory.</param>
    /// <param name="startDate">Initial date to look for files.</param>
    /// <param name="cancellationToken">Cancels the merge and stops the dotnet-coverage process.</param>
    /// <exception cref="RerunException">dotnet-coverage could not run or exited with a non-zero code</exception>
    public async Task Merge(RerunOptions options, string resultsDirectory, DateTime startDate, CancellationToken cancellationToken = default)
    {
        var arguments = CoverageMergeArguments.Build(options, GetCoverageFiles(resultsDirectory, startDate), resultsDirectory);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet-coverage",
            WorkingDirectory = "/"
        };
        foreach (var argument in arguments)
            processStartInfo.ArgumentList.Add(argument);
        var commandLine = CommandLineDisplay.Render(processStartInfo.FileName, arguments);

        Log.Debug($"working directory: {processStartInfo.WorkingDirectory}");
        Log.Debug($"forking {commandLine}");

        int exitCode;
        string error;
        try
        {
            var result = await ProcessRunner.RunAsync(processStartInfo, cancellationToken);
            exitCode = result.ExitCode;
            error = result.Error;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            exitCode = 1;
            error = e.Message;
        }

        if (exitCode != 0)
        {
            Log.Verbose(error);
            Log.Verbose($"Exit code {exitCode}.");
            throw new RerunException($"command failed:\n{commandLine} \nreason: {error}");
        }
    }

    private static IEnumerable<string> GetCoverageFiles(string resultsDirectory, DateTime startDate)
        => SearchPatterns.SelectMany(pattern =>
                        Directory.GetFiles(resultsDirectory, pattern, SearchOption.AllDirectories))
                        .Select(file => new FileInfo(file))
                        .Where(fileInfo => fileInfo.CreationTime >= startDate)
                        .Select(fileInfo => fileInfo.FullName);
}
