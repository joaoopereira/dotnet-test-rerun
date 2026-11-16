using System.IO.Abstractions.TestingHelpers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.TestLogger;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class LiveTestResultsReaderTests
{
    private const string Directory = "/live";

    [Fact]
    public void ReadNewResults_PrintsEachCompleteLineOnce()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("/live/a.jsonl", new MockFileData(
            "{\"Name\":\"A.Passes\",\"Passed\":true}\r\n" +
            "{\"Name\":\"A.Fails\",\"Passed\":false,\"ErrorMessage\":\"boom\",\"StackTrace\":\"at A\"}\n" +
            "{\"Name\":\"A.Partial\""));
        var reader = new LiveTestResultsReader(logger, fileSystem, Directory);

        // Act
        reader.ReadNewResults();
        reader.ReadNewResults();

        // Assert
        logger.Received(1).TestResult("A.Passes", true, null, null);
        logger.Received(1).TestResult("A.Fails", false, "boom", "at A");
        logger.ReceivedWithAnyArgs(2).TestResult(default!, default);
        reader.Count.Should().Be(2);
    }

    [Fact]
    public void ReadNewResults_PrintsLinesAppendedLater_AndSkipsUnreadableLines()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("/live/a.jsonl", new MockFileData("{\"Name\":\"A.First\",\"Passed\":true}\n{\"Name\":\"A.Sec"));
        var reader = new LiveTestResultsReader(logger, fileSystem, Directory);
        reader.ReadNewResults();

        // Act
        fileSystem.File.AppendAllText("/live/a.jsonl", "ond\",\"Passed\":true}\nnot json\n");
        fileSystem.AddFile("/live/b.jsonl", new MockFileData("{\"Name\":\"B.Other\",\"Passed\":false}\n"));
        reader.ReadNewResults();

        // Assert
        logger.Received(1).TestResult("A.First", true, null, null);
        logger.Received(1).TestResult("A.Second", true, null, null);
        logger.Received(1).TestResult("B.Other", false, null, null);
        reader.Count.Should().Be(3);
    }

    [Fact]
    public void ReadNewResults_MissingDirectory_PrintsNothing()
    {
        var logger = Substitute.For<ILogger>();
        var reader = new LiveTestResultsReader(logger, new MockFileSystem(), Directory);

        reader.ReadNewResults();

        reader.Count.Should().Be(0);
        logger.DidNotReceiveWithAnyArgs().TestResult(default!, default);
    }

    [Theory]
    [InlineData("Ns.Class.Method", "Ns.Class.Method", "Ns.Class.Method")]
    [InlineData("Ns.Class.Method", "Ns.Class.Method(x: 1)", "Ns.Class.Method(x: 1)")]
    [InlineData("Ns.Class.Method", "Method (1,2)", "Ns.Class.Method(1,2)")]
    [InlineData("Ns.Class.Method(1,2)", "Method(1,2)", "Ns.Class.Method(1,2)")]
    [InlineData("Ns.Class.Method", null, "Ns.Class.Method")]
    public void BuildName_AddsParametersMissingFromTheFullyQualifiedName(string fullyQualifiedName, string? displayName, string expected)
        => LiveTestResult.BuildName(fullyQualifiedName, displayName).Should().Be(expected);
}
