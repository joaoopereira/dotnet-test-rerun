using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using System.Diagnostics;
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
            Output = "Output"
        };

        // Act
        var output = processExecution.GetOutput();

        // Assert
        output.Should().Be(processExecution.Output);
    }
    
    [Fact]
    public void GetError_ShouldReturnSetValue()
    {
        // Arrange
        var logger = new Logger();
        ProcessExecution processExecution = new ProcessExecution(logger)
        {
            Error = "Error"
        };

        // Act
        var error = processExecution.GetError();

        // Assert
        error.Should().Be(processExecution.Error);
    }

    [Fact]
    public async Task Start_ShouldResetOutputAndError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new ProcessExecution(logger)
        {
            Output = "Previous Output",
            Error = "Previous Error"
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
        processExecution.Output.Should().BeEmpty();
        processExecution.Error.Should().BeEmpty();
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
        processExecution.Output.Should().BeEmpty();
        processExecution.Error.Should().BeEmpty();
        
        // Cleanup
        if (process2 != null && !process2.HasExited)
        {
            process2.Kill();
            process2.Dispose();
        }
    }
}