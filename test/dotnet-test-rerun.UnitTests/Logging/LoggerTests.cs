using dotnet.test.rerun.Logging;
using FluentAssertions;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Logging;

public class LoggerTests
{
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "")]
        [InlineData(LogLevel.Information, "")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Debug_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Debug(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Verbose_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Verbose(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "test message")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Information_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Information(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "test message")]
        [InlineData(LogLevel.Warning, "test message")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Warning_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Warning(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "test message")]
        [InlineData(LogLevel.Warning, "test message")]
        [InlineData(LogLevel.Error, "test message")]
        public void LoggerTests_Error_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Error(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }

        [Fact]
        public void LoggerTests_Exception_ShouldWrite()
        {
            // Arrange
            var testConsole = new TestConsole();
            var logger = new Logger(testConsole);
            var exception = new Exception("test exception");

            // Act
            logger.Exception(exception);

            // Assert
            testConsole.Output.Should().Contain(exception.Message);
        }

        [Fact]
        public void LoggerTests_Status_ShouldWrite()
        {
            // Arrange
            var testConsole = new TestConsole();
            var logger = new Logger(testConsole);
            var message = "test message";

            // Act
            logger.Status(message);

            // Assert
            testConsole.Output.Should().Contain(message);
        }

        [Fact]
        public void Render_Should_Write_Renderable()
        {
            // Arrange
            var testConsole = new TestConsole();
            var logger = new Logger(testConsole);
            var renderable = new Rule("test");

            // Act
            logger.Render(renderable);

            // Assert
            testConsole.Output.Should().Contain("───────────────────────────────────── test ─────────────────────────────────────");
        }
}