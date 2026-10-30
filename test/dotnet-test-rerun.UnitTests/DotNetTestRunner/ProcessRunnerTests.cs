using System.Diagnostics;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using AwesomeAssertions;
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
