using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;
using StatusContext = dotnet.test.rerun.Logging.StatusContext;

namespace dotnet_test_rerun.UnitTest.Logging;

[Collection("Sequential")]
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
        public void LoggerTests_StatusWithAction_ShouldWrite()
        {
            // Arrange
            var testConsole = new TestConsole();
            var logger = new Logger(testConsole);
            var message = "test message";
            var actionCalled = false;

            // Act
            logger.Status(message, delegate(StatusContext context) { actionCalled = true; });

            // Assert
            testConsole.Output.Should().Contain(message);
            actionCalled.Should().BeTrue();
        }

        [Fact]
        public void LoggerTests_StatusWithActionAndContextUpdate_ShouldUpdateStatus()
        {
            // Arrange
            var testConsole = new TestConsole();
            var logger = new Logger(testConsole);
            var message = "initial message";
            var updatedMessage = "updated message";
            var actionCalled = false;

            // Act
            logger.Status(message, context =>
            {
                actionCalled = true;
                context.Status(updatedMessage);
            });

            // Assert
            testConsole.Output.Should().Contain(updatedMessage);
            actionCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Debug, "✓ test.name")]
        [InlineData(LogLevel.Verbose, "✓ test.name")]
        [InlineData(LogLevel.Information, "✓ test.name")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_TestResult_Passed_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);

            // Act
            logger.TestResult("test.name", true);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }

        [Fact]
        public void LoggerTests_TestResult_Failed_ShouldWriteNameAndErrorDetails()
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();

            // Act
            logger.TestResult("test.name", false, "boom happened", "at test.name() in file.cs:line 1");

            // Assert
            var output = stringWriter.ToString();
            output.Should().Contain("✗ test.name");
            output.Should().Contain("boom happened");
            output.Should().Contain("at test.name() in file.cs:line 1");
        }

        [Fact]
        public void LoggerTests_TestResult_Failed_WithoutErrorDetails_ShouldOnlyWriteName()
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();

            // Act
            logger.TestResult("test.name", false);

            // Assert
            stringWriter.ToString().Trim().Should().Be("✗ test.name");
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