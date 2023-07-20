using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Abstractions;
using dotnet_test_rerun.IntegrationTests.Utilities;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using Moq;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class DotNetCoverageRunnerTests
{
    [Theory]
    [InlineData(CoverageFormat.Cobertura)]
    [InlineData(CoverageFormat.Xml)]
    [InlineData(CoverageFormat.Coverage)]
    public async Task Merge_GetNoErrors_Proceed(CoverageFormat? coverageFormat)
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution.Object);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = coverageFormat
        };

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processExecution.Setup(x => x.FetchOutput(It.IsAny<Process>())).Verifiable();
        processExecution.Setup(x => x.FetchError(It.IsAny<Process>())).Verifiable();
        processExecution.Setup(x => x.End(It.IsAny<Process>()))
            .ReturnsAsync(0).Verifiable();
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(new Process()).Verifiable();

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().NotThrowAsync<RerunException>();
        processExecution.VerifyAll();
    }
    
    [Fact]
    public async Task Merge_GetNoErrors_WithInvalidCoverageFormat()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution.Object);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = null!
        };

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithMessage("Exception of type 'System.ArgumentOutOfRangeException' was thrown. (Parameter 'MergeCoverageFormat')");
    }
    
    [Fact]
    public async Task Merge_GetErrorsWhileExecuting_Fail()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution.Object);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = CoverageFormat.Cobertura
        };
        var errorMessage = "Failed on dotnet coverage call";

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processExecution.Setup(x => x.FetchOutput(It.IsAny<Process>())).Verifiable();
        processExecution.Setup(x => x.FetchError(It.IsAny<Process>())).Verifiable();
        processExecution.Setup(x => x.End(It.IsAny<Process>()))
            .ReturnsAsync(1).Verifiable();
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(new Process()).Verifiable();
        processExecution.Setup(x => x.GetError())
            .Returns(errorMessage).Verifiable();

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"*{errorMessage}");
        processExecution.VerifyAll();
    }
    
    [Fact]
    public async Task Merge_GetExceptionWhileExecuting_Fail()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = new Mock<IProcessExecution>(MockBehavior.Strict);
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution.Object);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = CoverageFormat.Cobertura
        };
        var errorMessage = "Failed on dotnet coverage call";

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processExecution.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
            .ThrowsAsync(new Exception(errorMessage)).Verifiable();

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"*{errorMessage}");
        processExecution.VerifyAll();
    }
}