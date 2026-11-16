using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace dotnet.test.rerun.TestLogger;

/// <summary>
/// VSTest logger that writes each test result to its own file in the Directory parameter as soon as the test
/// finishes, so test-rerun can print it during the run. Each logger instance (one per test run, and several
/// run in parallel for multi-project or multi-target runs) writes its own file.
/// </summary>
[FriendlyName(LiveTestResult.LoggerFriendlyName)]
[ExtensionUri("logger://dotnet-test-rerun/live")]
public sealed class LiveTestLogger : ITestLoggerWithParameters
{
    private readonly object Gate = new();
    private StreamWriter? Writer;

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        // Without the Directory parameter there is nowhere to write to
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters)
    {
        if (!parameters.TryGetValue(LiveTestResult.DirectoryParameter, out var directory) || string.IsNullOrWhiteSpace(directory))
            return;

        Directory.CreateDirectory(directory);
        var stream = new FileStream(Path.Combine(directory, Guid.NewGuid().ToString("N") + LiveTestResult.FileExtension),
            FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        Writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
        events.TestResult += OnTestResult;
        events.TestRunComplete += (_, _) =>
        {
            lock (Gate)
            {
                Writer?.Dispose();
                Writer = null;
            }
        };
    }

    private void OnTestResult(object? sender, TestResultEventArgs e)
    {
        var result = e.Result;
        var line = JsonSerializer.Serialize(new LiveTestResult(
            LiveTestResult.BuildName(result.TestCase.FullyQualifiedName, result.DisplayName ?? result.TestCase.DisplayName),
            result.Outcome == TestOutcome.Passed,
            result.ErrorMessage,
            result.ErrorStackTrace));

        // JSON escapes new lines, so each result is exactly one line
        lock (Gate)
            Writer?.WriteLine(line);
    }
}
