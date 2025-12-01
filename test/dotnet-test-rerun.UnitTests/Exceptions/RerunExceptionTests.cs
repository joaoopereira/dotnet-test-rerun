using dotnet.test.rerun;
using dotnet.test.rerun.RerunCommand;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Exceptions;

public class RerunExceptionTests
{
    [Fact]
    public void RerunExceptionTests_ConstructorWithoutMessage_ThrowsException()
    {
        //Arrange
        var message = "Exception of type 'dotnet.test.rerun.RerunCommand.RerunException' was thrown.";
        
        //Act
        var exception = new RerunException();

        //Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
    }
    
    [Fact]
    public void RerunExceptionTests_ConstructorWithMessage_ThrowsExceptionWithGivenMessage()
    {
        //Arrange
        var message = "An error occurred while running the test.";
        
        //Act
        var exception = new RerunException(message);

        //Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void RerunExceptionTests_ConstructorWithMessageAndInnerException_ThrowsExceptionWithGivenMessageAndInnerException()
    {
        //Arrange
        var message = "An error occurred while running the test.";
        var innerException = new Exception("Inner exception message");
        
        //Act
        var exception = new RerunException(message, innerException);

        //Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}