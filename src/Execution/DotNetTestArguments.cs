using dotnet.test.rerun.Analyzers.Filters;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.TestLogger;

namespace dotnet.test.rerun.Execution;

/// <summary>
/// Builds the dotnet test arguments as a list, so the runtime quotes each one for the current OS.
/// </summary>
public static class DotNetTestArguments
{
    private const string TrxLogger = "trx";

    /// <summary>
    /// Extra logger added when --logPassedTests is set,
    /// so passed tests are printed to the console as they complete during execution.
    /// </summary>
    private const string PassedTestsLogger = "console;verbosity=detailed";

    /// <param name="liveResultsDirectory">When set, adds the live test logger, which writes each result to this folder as the test finishes.</param>
    public static IReadOnlyList<string> Build(RerunOptions options, RunRequest request, string? liveResultsDirectory = null)
    {
        var args = new List<string> { "test" };
        AddIfNotBlank(args, options.Path);
        AddOption(args, "--filter", IsAssembly(options.Path) ? VsTestFilter.DecodeQuotes(request.Filter) : request.Filter);
        AddOption(args, "--settings", options.Settings);
        foreach (var logger in options.Loggers)
            AddOption(args, "--logger", logger);
        // The rerun works from TRX results, so a TRX logger is always added when the user's loggers have none
        if (!options.Loggers.Any(logger => logger.StartsWith(TrxLogger, StringComparison.OrdinalIgnoreCase)))
            AddOption(args, "--logger", TrxLogger);
        if (options.LogPassedTests && !options.LogTestResults)
            AddOption(args, "--logger", PassedTestsLogger);
        if (liveResultsDirectory is not null)
        {
            AddOption(args, "--logger", $"{LiveTestResult.LoggerFriendlyName};{LiveTestResult.DirectoryParameter}={liveResultsDirectory}");
            // The logger assembly ships next to the tool
            AddOption(args, "--test-adapter-path", Path.GetDirectoryName(typeof(LiveTestResult).Assembly.Location));
        }
        AddFlag(args, "--no-build", options.NoBuild);
        AddFlag(args, "--no-restore", options.NoRestore);
        AddFlag(args, "--blame", options.Blame);
        AddOption(args, "-c", options.Configuration);
        AddOption(args, "--framework", request.Framework);
        AddOption(args, "-v", options.Verbosity?.ToString());
        AddOption(args, "--collect", options.Collector);
        foreach (var environmentVariable in options.EnvironmentVariables)
            AddOption(args, "-e", environmentVariable);
        if (!string.IsNullOrWhiteSpace(request.ResultsDirectory))
            AddOption(args, "--results-directory", request.ResultsDirectory);
        args.AddRange(options.MsBuildArguments);
        if (options.InlineRunSettings.Count > 0)
        {
            args.Add("--");
            args.AddRange(options.InlineRunSettings);
        }

        return args;
    }

    private static void AddIfNotBlank(List<string> args, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            args.Add(value);
    }

    private static void AddOption(List<string> args, string name, string? value)
    {
        if (value is null)
            return;

        args.Add(name);
        args.Add(value);
    }

    private static void AddFlag(List<string> args, string name, bool value)
    {
        if (value)
            args.Add(name);
    }

    // dotnet test passes the filter of a test assembly straight to VSTest, without MSBuild.
    private static bool IsAssembly(string? path)
        => path is not null && (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
}
