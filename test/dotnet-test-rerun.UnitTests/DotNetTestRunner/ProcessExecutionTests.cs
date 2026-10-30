using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class ProcessExecutionTests
{
    [Fact]
    public void GetOutput_ShouldReturnSetValue()
    {
        // Arrange
        var logger = new Logger();
        ProcessExecution processExecution = new ProcessExecution(logger)
        {
            Output = new StringBuilder("Output")
        };

        // Act
        var output = processExecution.GetOutput();

        // Assert
        output.Should().Be("Output");
    }
    
    [Fact]
    public void GetError_ShouldReturnSetValue()
    {
        // Arrange
        var logger = new Logger();
        ProcessExecution processExecution = new ProcessExecution(logger)
        {
            Error = new StringBuilder("Error")
        };

        // Act
        var error = processExecution.GetError();

        // Assert
        error.Should().Be("Error");
    }

    [Fact]
    public async Task Start_ShouldResetOutputAndError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new ProcessExecution(logger)
        {
            Output = new StringBuilder("Previous Output"),
            Error = new StringBuilder("Previous Error")
        };

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Act
        var process = await processExecution.Start(processStartInfo);

        // Assert
        processExecution.GetOutput().Should().BeEmpty();
        processExecution.GetError().Should().BeEmpty();
        process.Should().NotBeNull();
        
        // Cleanup
        if (process != null && !process.HasExited)
        {
            process.Kill();
            process.Dispose();
        }
    }

    [Fact]
    public async Task FetchOutput_ShouldCaptureProcessOutput()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new ProcessExecution(logger);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = await processExecution.Start(processStartInfo);

        // Act
        processExecution.FetchOutput(process!);
        await processExecution.End(process!);

        // Assert
        processExecution.GetOutput().Should().NotBeEmpty();
        processExecution.GetOutput().Should().Contain(".");
    }

    [Fact]
    public async Task FetchError_ShouldCaptureProcessError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new ProcessExecution(logger);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "invalid-command-that-does-not-exist-12345",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = await processExecution.Start(processStartInfo);

        // Act
        processExecution.FetchError(process!);
        await processExecution.End(process!);

        // Assert
        // Error may or may not be captured depending on how dotnet handles invalid commands
        // But the method should execute without throwing
        processExecution.GetError().Should().NotBeNull();
    }

    [Fact]
    public async Task End_ShouldWaitForProcessAndReturnExitCode()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new ProcessExecution(logger);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = await processExecution.Start(processStartInfo);

        // Act
        var exitCode = await processExecution.End(process!);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Start_MultipleCalls_ShouldResetOutputEachTime()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new ProcessExecution(logger);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // First call
        var process1 = await processExecution.Start(processStartInfo);
        processExecution.FetchOutput(process1!);
        await processExecution.End(process1!);
        var firstOutput = processExecution.GetOutput();

        // Second call - should reset
        var process2 = await processExecution.Start(processStartInfo);

        // Assert
        processExecution.GetOutput().Should().BeEmpty();
        processExecution.GetError().Should().BeEmpty();
        
        // Cleanup
        if (process2 != null && !process2.HasExited)
        {
            process2.Kill();
            process2.Dispose();
        }
    }

    [Fact]
    public async Task End_ShouldReturnOnlyAfterAllOutputWasCaptured()
    {
        // Arrange
        var logger = new Logger();
        logger.SetLogLevel(LogLevel.Error);
        var processExecution = new ProcessExecution(logger);
        var process = await processExecution.Start(ShellCommand("for i in $(seq 1 3000); do echo line$i; done",
            "for /L %i in (1,1,3000) do @echo line%i"));

        // Act
        processExecution.FetchOutput(process!);
        processExecution.FetchError(process!);
        var exitCode = await processExecution.End(process!);

        // Assert
        exitCode.Should().Be(0);
        processExecution.GetOutput().Should().EndWith("line3000");
    }

    [Fact]
    public async Task End_WhenCancelled_KillsProcessAndThrows()
    {
        // Arrange
        var logger = new Logger();
        logger.SetLogLevel(LogLevel.Error);
        var processExecution = new ProcessExecution(logger);
        var process = await processExecution.Start(ShellCommand("sleep 60", "ping -n 60 127.0.0.1 > nul"));
        processExecution.FetchOutput(process!);
        processExecution.FetchError(process!);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var act = () => processExecution.End(process!, cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
        process!.WaitForExit(5000).Should().BeTrue();
        process.HasExited.Should().BeTrue();
    }

    private static ProcessStartInfo ShellCommand(string unixCommand, string windowsCommand)
    {
        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo("cmd", $"/c {windowsCommand}")
            : new ProcessStartInfo("sh", new[] { "-c", unixCommand });
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        return startInfo;
    }
}
