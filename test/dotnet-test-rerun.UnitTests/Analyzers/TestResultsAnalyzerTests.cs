using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetTestRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Analyzers;

public class TestResultsAnalyzerTests
{
    private static readonly IFileSystem FileSystem = new FileSystem();
    private static readonly ILogger Logger = new Logger();

    private TestResultsAnalyzer TestResultsAnalyzer = new TestResultsAnalyzer(Logger);
    private readonly IDirectoryInfo ResultsDirectory = FileSystem.DirectoryInfo.New("../../../Fixtures/RerunCommand/");

    [Fact]
    public void GetFailedTestsFilter_XUnit_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void GetFailedTestsFilter_XUnit_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~XUnitExample.SimpleTest.SimpleStringCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_XUnit_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare | FullyQualifiedName~XUnitExample.UnitTest1.SimpleStringCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleStringCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleStringCompare | FullyQualifiedName~SimpleNumberCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleStringCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = TestResultsAnalyzer.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleNumberCompare | FullyQualifiedName~SimpleStringCompare");
    }
    
    [Fact]
    public void GetTrxFile_FromValidDirectory_ReturnFile()
    {
        //Act
        var result = TestResultsAnalyzer.GetTrxFile(ResultsDirectory);

        //Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("XUnitTrxFileWithSeveralFailedTests.trx");
    }
    
    [Fact]
    public void GetTrxFile_FromDirectoryWithoutFiles_ReturnNoFile()
    {
        //Arrange
        IDirectoryInfo dir = FileSystem.DirectoryInfo.New(".");
        
        //Act
        var result = TestResultsAnalyzer.GetTrxFile(dir);

        //Assert
        result.Should().BeNull();
    }
}