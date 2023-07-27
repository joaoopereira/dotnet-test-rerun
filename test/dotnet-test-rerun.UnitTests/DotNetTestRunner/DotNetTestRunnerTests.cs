using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using Moq;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class DotNetTestRunnerTests
{
    [Fact]
    public void GetErrorCode_GetDefaultValue()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution.Object);

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
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution.Object);
        processExecution.Setup(x => x.FetchOutput(It.IsAny<Process>()));
        processExecution.Setup(x => x.FetchError(It.IsAny<Process>()));
        processExecution.Setup(x => x.End(It.IsAny<Process>()))
            .ReturnsAsync(0);
        processExecution.Setup(x => x.GetOutput())
            .Returns("No errors");
        processExecution.Setup(x => x.GetError())
            .Returns(string.Empty);
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.Success);
    }
    
    [Fact]
    public async Task Test_GetErrors_UnknownError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution.Object);
        processExecution.Setup(x => x.FetchOutput(It.IsAny<Process>()));
        processExecution.Setup(x => x.FetchError(It.IsAny<Process>()));
        processExecution.Setup(x => x.End(It.IsAny<Process>()))
            .ReturnsAsync(1);
        processExecution.Setup(x => x.GetOutput())
            .Returns(string.Empty);
        processExecution.Setup(x => x.GetError())
            .Returns("Unexpected error");
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(new Process());

        // Act
        var act = () => dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage("command:\ndotnet test  --results-directory \"resultsDirectory\"");
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.Error);
    }
    
    [Fact]
    public async Task Test_GetErrors_WellKnownError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution.Object);
        processExecution.Setup(x => x.FetchOutput(It.IsAny<Process>()));
        processExecution.Setup(x => x.FetchError(It.IsAny<Process>()));
        processExecution.Setup(x => x.End(It.IsAny<Process>()))
            .ReturnsAsync(1);
        processExecution.Setup(x => x.GetOutput())
            .Returns(String.Empty);
        processExecution.Setup(x => x.GetError())
            .Returns("No test source files were specified.");
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(new Process());

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
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution.Object);
        processExecution.Setup(x => x.FetchOutput(It.IsAny<Process>()));
        processExecution.Setup(x => x.FetchError(It.IsAny<Process>()));
        processExecution.Setup(x => x.End(It.IsAny<Process>()))
            .ReturnsAsync(1);
        processExecution.Setup(x => x.GetOutput())
            .Returns("FirstLine\nFailed!  - Failed:\nThirdLine");
        processExecution.Setup(x => x.GetError())
            .Returns(string.Empty);
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(new Process());

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.FailedTests);
    }
}