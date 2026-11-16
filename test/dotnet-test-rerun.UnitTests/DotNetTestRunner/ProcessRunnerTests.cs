using System.Diagnostics;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class ProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_ReturnsExitCodeAndOutput()
    {
        // Arrange
        var processRunner = new ProcessRunner(QuietLogger());

        // Act
        var result = await processRunner.RunAsync(new ProcessStartInfo("dotnet", "--version"));

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain(".");
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_CapturesStandardError()
    {
        // Arrange
        var processRunner = new ProcessRunner(QuietLogger());

        // Act
        var result = await processRunner.RunAsync(ShellCommand("echo oops 1>&2; exit 3", "echo oops 1>&2 & exit 3"));

        // Assert
        result.ExitCode.Should().Be(3);
        result.Error.Should().Contain("oops");
    }

    [Fact]
    public async Task RunAsync_EachCallReturnsOnlyItsOwnOutput()
    {
        // Arrange
        var processRunner = new ProcessRunner(QuietLogger());

        // Act
        var first = await processRunner.RunAsync(ShellCommand("echo first", "echo first"));
        var second = await processRunner.RunAsync(ShellCommand("echo second", "echo second"));

        // Assert
        first.Output.Should().Contain("first").And.NotContain("second");
        second.Output.Should().Contain("second").And.NotContain("first");
    }

    [Fact]
    public async Task RunAsync_ReturnsOnlyAfterAllOutputWasCaptured()
    {
        // Arrange
        var processRunner = new ProcessRunner(QuietLogger());

        // Act
        var result = await processRunner.RunAsync(ShellCommand("for i in $(seq 1 3000); do echo line$i; done",
            "for /L %i in (1,1,3000) do @echo line%i"));

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().EndWith("line3000");
    }

    [Fact]
    public async Task RunAsync_WhenCancelled_KillsProcessAndThrows()
    {
        // Arrange
        var processRunner = new ProcessRunner(QuietLogger());
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var act = () => processRunner.RunAsync(ShellCommand("sleep 60", "ping -n 60 127.0.0.1 > nul"), cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task RunAsync_WhenCancelled_KillsTheRunningProcess()
    {
        // shortcut: needs a POSIX shell to report the pid of the process to kill; CI runs on Linux
        if (OperatingSystem.IsWindows())
            return;

        // Arrange
        var logger = Substitute.For<ILogger>();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var processId = new TaskCompletionSource<int>();
        logger.When(log => log.Verbose(Arg.Any<string>())).Do(call =>
        {
            // Cancel once the process is running, as Ctrl+C would
            if (int.TryParse(call.Arg<string>(), out var id) && processId.TrySetResult(id))
                cancellation.Cancel();
        });
        var processRunner = new ProcessRunner(logger);

        // Act
        var act = () => processRunner.RunAsync(new ProcessStartInfo("sh", new[] { "-c", "echo $$; exec sleep 60" }), cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        logger.Received(1).Warning("Cancellation requested. Stopping the running process...");
        ProcessHasExited(await processId.Task).Should().BeTrue();
    }

    private static bool ProcessHasExited(int id)
    {
        try
        {
            using var process = Process.GetProcessById(id);
            return process.WaitForExit(TimeSpan.FromSeconds(10));
        }
        catch (ArgumentException)
        {
            // No process with this id anymore
            return true;
        }
    }

    private static Logger QuietLogger()
    {
        var logger = new Logger();
        logger.SetLogLevel(LogLevel.Error);
        return logger;
    }

    private static ProcessStartInfo ShellCommand(string unixCommand, string windowsCommand)
        => OperatingSystem.IsWindows()
            ? new ProcessStartInfo("cmd", $"/c {windowsCommand}")
            : new ProcessStartInfo("sh", new[] { "-c", unixCommand });
}
