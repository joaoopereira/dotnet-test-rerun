using System.IO.Abstractions;
using dotnet.test.rerun;
using dotnet.test.rerun.Logging;
using FluentAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.RerunCommand;

public class RerunCommandTests
{
    private static readonly IFileSystem FileSystem = new FileSystem();
    private static readonly ILogger Logger = new Logger();

    private dotnet.test.rerun.RerunCommand RerunCommand = new dotnet.test.rerun.RerunCommand(Logger,
        new RerunCommandConfiguration(),
        new dotnet.test.rerun.dotnet(Logger),
        FileSystem);
    private readonly IDirectoryInfo ResultsDirectory = FileSystem.DirectoryInfo.New("../../../Fixtures/RerunCommand/");

    [Fact]
    public void GetFailedTestsFilter_XUnit_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void GetFailedTestsFilter_XUnit_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~XUnitExample.SimpleTest.SimpleStringCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_XUnit_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("XUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare(number: 1) | " +
                           "FullyQualifiedName~XUnitExample.UnitTest1.SimpleStringCompare | " +
                           "FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare(number: 3) | " +
                           "FullyQualifiedName~XUnitExample.UnitTest1.SimpleNumberCompare(number: 4)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleStringCompare()");
    }
    
    [Fact]
    public void GetFailedTestsFilter_NUnit_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("NUnitTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleStringCompare() | " +
                           "FullyQualifiedName~SimpleNumberCompare(1,2) | " +
                           "FullyQualifiedName~SimpleNumberCompare(3,2) | " +
                           "FullyQualifiedName~SimpleNumberCompare(4,2)");
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_NoFailedTests_ReturnEmpty()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithAllTestsPassing.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_OneFailedTest_ReturnOne()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithOneFailedTest.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleStringCompare");
    }
    
    [Fact]
    public void GetFailedTestsFilter_MsTest_SeveralFailedTest_ReturnSeveral()
    {
        //Arrange
        var trxFile = ResultsDirectory.EnumerateFiles("MsTestTrxFileWithSeveralFailedTests.trx").OrderBy(f => f.Name).LastOrDefault();

        //Act
        var result = RerunCommand.GetFailedTestsFilter(trxFile!);

        //Assert
        result.Should().Be("FullyQualifiedName~SimpleNumberCompare (1,2) | " +
                           "FullyQualifiedName~SimpleStringCompare | " +
                           "FullyQualifiedName~SimpleNumberCompare (3,2) | " +
                           "FullyQualifiedName~SimpleNumberCompare (4,2)");
    }
}