using System.Diagnostics;
using System.IO.Abstractions;
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
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner, new FileSystem());
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
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner, new FileSystem());
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
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner, new FileSystem());
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
        var dotNetCoverageRunner = new DotNetCoverageRunner(logger, processRunner, new FileSystem());
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

    [Fact]
    public async Task Merge_PassesOnlyCoverageFilesCreatedSinceStartDate()
    {
        // Arrange
        var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
        var resultsDirectory = fileSystem.Path.GetFullPath("results");
        fileSystem.AddFile(fileSystem.Path.Combine(resultsDirectory, "old.coverage"),
            new System.IO.Abstractions.TestingHelpers.MockFileData("") { CreationTime = new DateTime(2020, 1, 1) });
        fileSystem.AddFile(fileSystem.Path.Combine(resultsDirectory, "run", "new.cobertura.xml"),
            new System.IO.Abstractions.TestingHelpers.MockFileData("") { CreationTime = new DateTime(2030, 1, 1) });
        var processRunner = Substitute.For<IProcessRunner>();
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, string.Empty, string.Empty));
        var dotNetCoverageRunner = new DotNetCoverageRunner(new Logger(), processRunner, fileSystem);

        // Act
        await dotNetCoverageRunner.Merge(new RerunOptions { MergeCoverageFormat = CoverageFormat.Cobertura },
            resultsDirectory, new DateTime(2025, 1, 1));

        // Assert
        await processRunner.Received(1).RunAsync(
            Arg.Is<ProcessStartInfo>(psi => psi.ArgumentList.Contains(fileSystem.Path.Combine(resultsDirectory, "run", "new.cobertura.xml")) &&
                                            !psi.ArgumentList.Any(argument => argument.EndsWith("old.coverage"))),
            Arg.Any<CancellationToken>());
    }
}
