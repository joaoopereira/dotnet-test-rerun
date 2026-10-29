using System.Diagnostics;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class DotNetTestRunnerTests
{
    [Fact]
    public void GetErrorCode_GetDefaultValue()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);

        // Act
        var error = dotNetTestRunner.GetErrorCode();

        // Assert
        error.Should().Be(ErrorCode.Success);
    }
    
    [Fact]
    public async Task Test_GetNoErrors_Proceed()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        processExecution.End(Arg.Any<Process>())
            .Returns(0);
        processExecution.GetOutput()
            .Returns("No errors");
        processExecution.GetError()
            .Returns(string.Empty);
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.Success);
    }
    
    [Fact]
    public async Task Test_GetErrors_UnknownError()
    {
        // Arrange
        const string errorMessage = "Unexpected error";
        
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        processExecution.End(Arg.Any<Process>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns(string.Empty);
        processExecution.GetError()
            .Returns(errorMessage);
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        var act = () => dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"command: dotnet test --results-directory \"resultsDirectory\" exited with code 1. \n\nError message: {errorMessage}");
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.Error);
    }
    
    [Fact]
    public async Task Test_GetErrors_WellKnownError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        processExecution.End(Arg.Any<Process>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns(String.Empty);
        processExecution.GetError()
            .Returns("No test source files were specified.");
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.WellKnownError);
    }
    
    [Fact]
    public async Task Test_GetErrors_FailedTests()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        processExecution.End(Arg.Any<Process>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns("FirstLine\nFailed!  - Failed:\nThirdLine");
        processExecution.GetError()
            .Returns(string.Empty);
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.FailedTests);
    }
    
    [Fact]
    public async Task Test_TestHostCrash_TreatedAsFailedTests()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        processExecution.End(Arg.Any<Process>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns("Test host process crashed\nTest Run Aborted.");
        processExecution.GetError()
            .Returns(string.Empty);
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.FailedTests);
    }
    
    [Fact]
    public async Task Test_TestRunAborted_TreatedAsFailedTests()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        processExecution.End(Arg.Any<Process>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns("The active test run was aborted. Reason: Test host process crashed");
        processExecution.GetError()
            .Returns(string.Empty);
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.FailedTests);
    }
    
    [Fact]
    public async Task GetLastTestOutput_ReturnsConsoleOutput()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution);
        const string expectedOutput = "Test host process crashed\nThe test running when the crash occurred:\nTests1.TestClass.TestMethod";
        
        processExecution.End(Arg.Any<Process>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns(expectedOutput);
        processExecution.GetError()
            .Returns(string.Empty);
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");
        var output = dotNetTestRunner.GetLastTestOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }
}