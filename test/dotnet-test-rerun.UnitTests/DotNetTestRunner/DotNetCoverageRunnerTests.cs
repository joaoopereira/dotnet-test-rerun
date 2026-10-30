using System.Diagnostics;
using dotnet_test_rerun.Common.Utilities;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun;
using AwesomeAssertions;
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
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunOptions()
        {
            MergeCoverageFormat = coverageFormat
        };

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));

        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, string.Empty, string.Empty));

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().NotThrowAsync<RerunException>();
        await processRunner.Received(1).RunAsync(
            Arg.Is<ProcessStartInfo>(psi => psi.FileName == "dotnet-coverage" && psi.ArgumentList[0] == "merge"),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Merge_GetNoErrors_WithInvalidCoverageFormat()
    {
        // Arrange
        var logger = new Logger();
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunOptions()
        {
            MergeCoverageFormat = null!
        };

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithMessage("Specified argument was out of the range of valid values. (Parameter 'MergeCoverageFormat')");
    }
    
    [Fact]
    public async Task Merge_GetErrorsWhileExecuting_Fail()
    {
        // Arrange
        var logger = new Logger();
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunOptions()
        {
            MergeCoverageFormat = CoverageFormat.Cobertura
        };
        var errorMessage = "Failed on dotnet coverage call";

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(1, string.Empty, errorMessage));

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"*{errorMessage}");
        await processRunner.Received(1).RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Merge_GetExceptionWhileExecuting_Fail()
    {
        // Arrange
        var logger = new Logger();
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner);
        var _dir = TestUtilities.GetTmpDirectory();
        var configuration = new RerunOptions()
        {
            MergeCoverageFormat = CoverageFormat.Cobertura
        };
        var errorMessage = "Failed on dotnet coverage call";

        TestUtilities.CopyFixture("DotNetCoverage", new DirectoryInfo(_dir));
        
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var act = () => dotNetCoverageRunner.Merge(configuration, _dir, DateTime.MinValue);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"*{errorMessage}");
    }
}