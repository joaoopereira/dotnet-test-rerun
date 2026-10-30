using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.Configuration;

/// <summary>
/// The options of a test-rerun invocation. Created once from the command line and never changed.
/// Defaults match the command-line defaults.
/// </summary>
public sealed record RerunOptions
{
    public string? Path { get; init; }
    public string? Filter { get; init; }
    public string? Settings { get; init; }
    public IReadOnlyList<string> Loggers { get; init; } = new[] { "trx" };
    public string ResultsDirectory { get; init; } = ".";
    public int RerunMaxAttempts { get; init; } = 3;
    public int RerunMaxFailedTests { get; init; } = -1;
    public double RerunFailedThreshold { get; init; } = -1;
    public LogLevel LogLevel { get; init; } = LogLevel.Verbose;
    public bool NoBuild { get; init; }
    public bool NoRestore { get; init; }
    public TimeSpan Delay { get; init; }
    public bool Blame { get; init; }
    public bool DeleteReportFiles { get; init; }
    public string? Collector { get; init; }
    public CoverageFormat? MergeCoverageFormat { get; init; }
    public string? Configuration { get; init; }
    public LoggerVerbosity? Verbosity { get; init; }
    public string? Framework { get; init; }
    public IReadOnlyList<string> MsBuildArguments { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> InlineRunSettings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EnvironmentVariables { get; init; } = Array.Empty<string>();
    public bool LogPassedTests { get; init; }
    public bool LogTestResults { get; init; }
}
