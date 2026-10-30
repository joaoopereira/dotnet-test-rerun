using System.Diagnostics;
using System.IO.Abstractions;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.DotNetRunner;

public class DotNetCoverageRunner : IDotNetCoverageRunner
{
    private static readonly string[] SearchPatterns = { "*.coverage", "*.cobertura*" };
    private readonly ILogger Log;
    private readonly IProcessRunner ProcessRunner;
    private readonly IFileSystem FileSystem;

    public DotNetCoverageRunner(ILogger logger,
        IProcessRunner processRunner,
        IFileSystem fileSystem)
    {
        Log = logger;
        ProcessRunner = processRunner;
        FileSystem = fileSystem;
    }

    /// <summary>
    /// Merges the coverage reports found in <paramref name="searchDirectories"/> (and their subdirectories)
    /// </summary>
    /// <param name="options">The test-rerun options.</param>
    /// <param name="searchDirectories">The directories of the test runs, e.g. one per attempt.</param>
    /// <param name="outputDirectory">Where the merged report is written.</param>
    /// <param name="cancellationToken">Cancels the merge and stops the dotnet-coverage process.</param>
    /// <exception cref="RerunException">dotnet-coverage could not run or exited with a non-zero code</exception>
    public async Task Merge(RerunOptions options, IEnumerable<string> searchDirectories, string outputDirectory, CancellationToken cancellationToken = default)
    {
        var arguments = CoverageMergeArguments.Build(options, GetCoverageFiles(searchDirectories), outputDirectory);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet-coverage",
            WorkingDirectory = FileSystem.Directory.GetCurrentDirectory()
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

    private IEnumerable<string> GetCoverageFiles(IEnumerable<string> searchDirectories)
        => searchDirectories
            .Where(FileSystem.Directory.Exists)
            .SelectMany(directory => SearchPatterns.SelectMany(pattern =>
                FileSystem.Directory.GetFiles(directory, pattern, SearchOption.AllDirectories)))
            .Select(file => FileSystem.Path.GetFullPath(file))
            .Distinct()
            .ToArray();
}
