using System.Diagnostics;
using System.IO.Abstractions;
using dotnet_test_rerun.Common.Utilities;
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
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());

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
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
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
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
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
        await act.Should().ThrowAsync<RerunException>().WithMessage($"command: dotnet test --logger \"trx\" --results-directory \"resultsDirectory\" exited with code 1. \n\nError message: {errorMessage}");
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.Error);
    }
    
    [Fact]
    public async Task Test_GetErrors_WellKnownError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
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
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
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
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
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
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
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
    public async Task Test_NonEnglishOutput_WithTrxProduced_TreatedAsFailedTests()
    {
        // Arrange
        var logger = new Logger();
        var resultsDirectory = TestUtilities.GetTmpDirectory();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());
        processExecution.End(Arg.Any<Process>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                File.WriteAllText(Path.Combine(resultsDirectory, "run.trx"), "<TestRun />");
                return 1;
            });
        processExecution.GetOutput()
            .Returns("Com falha!  - Com falha:     1, Aprovado:     1");
        processExecution.GetError()
            .Returns(string.Empty);

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), resultsDirectory);

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.FailedTests);
    }

    [Fact]
    public async Task Test_NonEnglishOutput_WithoutTrx_Throws()
    {
        // Arrange
        var logger = new Logger();
        var resultsDirectory = TestUtilities.GetTmpDirectory();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());
        processExecution.End(Arg.Any<Process>(), Arg.Any<CancellationToken>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns("Erro de compilação");
        processExecution.GetError()
            .Returns(string.Empty);

        // Act
        var act = () => dotNetTestRunner.Test(new RerunCommandConfiguration(), resultsDirectory);

        // Assert
        await act.Should().ThrowAsync<RerunException>();
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.Error);
    }

    [Fact]
    public async Task Test_WellKnownErrorWithinCapturedStderr_TreatedAsWellKnownError()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(logger, processExecution, new FileSystem());
        processExecution.Start(Arg.Any<ProcessStartInfo>())
            .Returns(new Process());
        processExecution.End(Arg.Any<Process>(), Arg.Any<CancellationToken>())
            .Returns(1);
        processExecution.GetOutput()
            .Returns(string.Empty);
        // captured stderr lines are prefixed with a new line
        processExecution.GetError()
            .Returns("\nNo test source files were specified.");

        // Act
        await dotNetTestRunner.Test(new RerunCommandConfiguration(), "resultsDirectory");

        // Assert
        dotNetTestRunner.GetErrorCode().Should().Be(ErrorCode.WellKnownError);
    }
}
