using System.Diagnostics;
using dotnet_test_rerun.IntegrationTests.Utilities;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = coverageFormat
        };

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));

        processExecution.End(Arg.Any<Process>()).Returns(0);
        processExecution.Start(Arg.Any<ProcessStartInfo>()).Returns(new Process());

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().NotThrowAsync<RerunException>();
        processExecution.Received(1).FetchOutput(Arg.Any<Process>());
        processExecution.Received(1).FetchError(Arg.Any<Process>());
        await processExecution.Received(1).End(Arg.Any<Process>());
        await processExecution.Received(1).Start(Arg.Any<ProcessStartInfo>());
    }
    
    [Fact]
    public async Task Merge_GetNoErrors_WithInvalidCoverageFormat()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution);
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
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = CoverageFormat.Cobertura
        };
        var errorMessage = "Failed on dotnet coverage call";

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processExecution.End(Arg.Any<Process>()).Returns(1);
        processExecution.Start(Arg.Any<ProcessStartInfo>())!.Returns(new Process());
        processExecution.GetError().Returns(errorMessage);

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"*{errorMessage}");
        processExecution.Received(1).FetchOutput(Arg.Any<Process>());
        processExecution.Received(1).FetchError(Arg.Any<Process>());
        await processExecution.Received(1).End(Arg.Any<Process>());
        await processExecution.Received(1).Start(Arg.Any<ProcessStartInfo>());
        processExecution.Received(2).GetError();
    }
    
    [Fact]
    public async Task Merge_GetExceptionWhileExecuting_Fail()
    {
        // Arrange
        var logger = new Logger();
        var processExecution = Substitute.For<IProcessExecution>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processExecution);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunCommandConfiguration()
        {
            MergeCoverageFormat = CoverageFormat.Cobertura
        };
        var errorMessage = "Failed on dotnet coverage call";

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processExecution.Start(Arg.Any<ProcessStartInfo>()).Throws(new Exception(errorMessage));

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"*{errorMessage}");
    }
}