using System.IO.Abstractions;
using dotnet_test_rerun.Common.Utilities;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.Logging;
using FluentAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Analyzers;

public class TestResultsAnalyzerTests
{
    private static readonly IFileSystem FileSystem = new FileSystem();
    private static readonly ILogger Logger = new Logger();

    private TestResultsAnalyzer TestResultsAnalyzer = new (Logger);
    private readonly IDirectoryInfo ResultsDirectory = FileSystem.DirectoryInfo.New("../../../Fixtures/RerunCommand/");

    [Fact]
    public void GetFailedTestsFilter_XUnit_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.Should().HaveCount(0);
    }
    
    [Fact]
    public void GetFailedTestsFilter_XUnit_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~XUnitExample.SimpleTest.SimpleStringCompare)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_XUnit_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare&DisplayName~number: 1) | (FullyQualifiedName~XUnitExample.UnitTest1.SimpleStringCompare) | (FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare&DisplayName~number: 3) | (FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare&DisplayName~number: 4)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.Should().HaveCount(0);
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~NUnitTestExample.Tests.SimpleStringCompare)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_OneFailedTest_WithSameNameAsPassed_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithOneFailedTestWithSameName.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~NUnitTestExample.Tests.SimpleStringCompare)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~NUnitTestExample.Tests.SimpleStringCompare) | (FullyQualifiedName~NUnitTestExample.Tests.SimpleNumberCompare)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.Should().HaveCount(0);
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~MSTestExample.UnitTest1.SimpleStringCompare)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~MSTestExample.UnitTest1.SimpleNumberCompare) | (FullyQualifiedName~MSTestExample.UnitTest1.SimpleStringCompare)");
    }
    
    [Fact]
    public void GetTrxFiles_FromValidDirectory_ReturnFile()
    {
        //Arrange
        DateTime start = DateTime.MinValue;
        var testDirPath = TestUtilities.GetTmpDirectory();
        IDirectoryInfo testDir = FileSystem.DirectoryInfo.New(testDirPath);
        TestUtilities.CopyFixtureFile("RerunCommand", "XUnitTrxFileWithSeveralFailedTests.trx", new DirectoryInfo(testDirPath));
        
        //Act
        var result = TestResultsAnalyzer.GetTrxFiles(testDir, start);

        //Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("XUnitTrxFileWithSeveralFailedTests.trx");
    }
    
    
    [Fact]
    public void GetTrxFiles_FromValidDirectory_ReturnFiles()
    {
        //Act
        var result = TestResultsAnalyzer.GetTrxFiles(ResultsDirectory, DateTime.MinValue);

        //Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(10);
    }
    
    [Fact]
    public void GetTrxFiles_FromDirectoryWithoutFiles_ReturnNoFile()
    {
        //Arrange
        IDirectoryInfo dir = FileSystem.DirectoryInfo.New(".");
        DateTime start = DateTime.MinValue;
        
        //Act
        var result = TestResultsAnalyzer.GetTrxFiles(dir, start);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void AddLastTrxFile_FromValidDirectory_ReturnFiles()
    {
        //Arrange
        DateTime start = DateTime.MinValue;
        var testDirPath = TestUtilities.GetTmpDirectory();
        IDirectoryInfo testDir = FileSystem.DirectoryInfo.New(testDirPath);
        TestUtilities.CopyFixtureFile("RerunCommand", "MsTestTrxFileWithOneFailedTest.trx", new DirectoryInfo(testDirPath));
        
        //Act
        TestResultsAnalyzer.AddLastTrxFiles(testDir, start);

        //Assert
        var reportFiles = TestResultsAnalyzer.GetReportFiles();
        reportFiles.Should().NotBeNull();
        reportFiles.Should().HaveCount(1);
        var trxFiles = TestResultsAnalyzer.GetTrxFiles(testDir, start);
        trxFiles.Should().HaveCount(1);
        reportFiles.ElementAt(0).Should().Be(trxFiles[0].FullName);
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_NoFailedTests_WithTwoFiles_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { trxFile!, trxFile!});

        //Assert
        result.Filters.Should().HaveCount(0);
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_FailedTests_InTwoFiles_ReturnAll()
    {
        //Arrange
        var firstTrxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();
        var secondTrxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { firstTrxFile!, secondTrxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~NUnitTestExample.Tests.SimpleStringCompare) | (FullyQualifiedName~NUnitTestExample.Tests.SimpleStringCompare) | (FullyQualifiedName~NUnitTestExample.Tests.SimpleNumberCompare)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_OneFileWithFailing_AnotherWithPassing_ReturnOne()
    {
        //Arrange
        var firstTrxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();
        var secondTrxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(new[] { firstTrxFile!, secondTrxFile!});

        //Assert
        result.Filters.ElementAt(0).Key.Should().Be("net6.0");
        result.Filters.ElementAt(0).Value.Filter.Should().Be("(FullyQualifiedName~NUnitTestExample.Tests.SimpleStringCompare)");
    }
}