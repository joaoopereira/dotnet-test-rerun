using System.IO.Abstractions;
using System.Text;
using System.Text.Json;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.TestLogger;

namespace dotnet.test.rerun.DotNetRunner;

/// <summary>
/// Prints the results that the live test logger writes to <see cref="Directory"/> while dotnet test runs.
/// </summary>
public sealed class LiveTestResultsReader
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(200);
    private readonly ILogger Log;
    private readonly IFileSystem FileSystem;
    private readonly Dictionary<string, long> ReadOffsets = new();

    public LiveTestResultsReader(ILogger logger, IFileSystem fileSystem, string directory)
    {
        Log = logger;
        FileSystem = fileSystem;
        Directory = directory;
    }

    public string Directory { get; }

    /// <summary>
    /// Number of test results printed so far.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Prints new results until cancelled, then prints what is left.
    /// </summary>
    public async Task WatchAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ReadNewResults();
            try
            {
                await Task.Delay(PollInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        ReadNewResults();
    }

    /// <summary>
    /// Prints the complete lines written since the last call. A line still being written is left for the next call.
    /// </summary>
    public void ReadNewResults()
    {
        if (!FileSystem.Directory.Exists(Directory))
            return;

        foreach (var file in FileSystem.Directory.GetFiles(Directory, "*" + LiveTestResult.FileExtension).OrderBy(file => file))
        {
            try
            {
                ReadNewLines(file);
            }
            catch (IOException e)
            {
                // Tried again on the next call
                Log.Debug($"Could not read {file}: {e.Message}");
            }
        }
    }

    private void ReadNewLines(string file)
    {
        ReadOffsets.TryGetValue(file, out var offset);
        using var stream = FileSystem.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        if (stream.Length <= offset)
            return;

        stream.Seek(offset, SeekOrigin.Begin);
        var bytes = new byte[stream.Length - offset];
        // Stream.ReadExactly needs .NET 7
        var read = 0;
        int count;
        while (read < bytes.Length && (count = stream.Read(bytes, read, bytes.Length - read)) > 0)
            read += count;
        if (read < bytes.Length)
            throw new EndOfStreamException();
        var end = Array.LastIndexOf(bytes, (byte)'\n');
        if (end < 0)
            return;

        ReadOffsets[file] = offset + end + 1;
        foreach (var line in Encoding.UTF8.GetString(bytes, 0, end).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            Print(line);
    }

    private void Print(string line)
    {
        LiveTestResult? result;
        try
        {
            result = JsonSerializer.Deserialize<LiveTestResult>(line);
        }
        catch (JsonException e)
        {
            Log.Debug($"Ignoring an unreadable live test result: {e.Message}");
            return;
        }

        if (result is null)
            return;

        Log.TestResult(result.Name, result.Passed, result.ErrorMessage, result.StackTrace);
        Count++;
    }
}
