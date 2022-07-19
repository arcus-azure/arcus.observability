using System;
using System.Collections;
using System.Collections.Generic;
using Arcus.Observability.Tests.Core;
using Bogus;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Hosting;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    public class GeneralLoggingTests : IDisposable
    {
        private static readonly Faker BogusGenerator = new Faker();

        private readonly LoggingLevelSwitch _logLevelSwitch;
        private readonly InMemoryLogSink _spySink;
        private readonly SerilogLoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private LogEventLevel? _filteredOutEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralLoggingTests" /> class.
        /// </summary>
        public GeneralLoggingTests()
        {
            _logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
            _spySink = new InMemoryLogSink();
            
            Logger serilogLogger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_logLevelSwitch)
                .Filter.ByExcluding(logEvent => logEvent.Level == _filteredOutEvent)
                .WriteTo.Sink(_spySink)
                .CreateLogger();

            _loggerFactory = new SerilogLoggerFactory(serilogLogger);
            _logger = _loggerFactory.CreateLogger<GeneralLoggingTests>();
        }

        [Fact]
        public void LogInformationWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogInformation(message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Information, logEvent.Level);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogInformationWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };
            
            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogInformation(message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogInformationWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogInformation(message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Information, logEvent.Level);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogInformationWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogInformation(message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogTraceWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogTrace(message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Verbose, logEvent.Level);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogTraceWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogTrace(message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogTraceWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogTrace(message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Verbose, logEvent.Level);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogTraceWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogTrace(message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogErrorWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogError(message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Error, logEvent.Level);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogErrorWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogError(message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogErrorWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogError(message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Error, logEvent.Level);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogErrorWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogError(message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogErrorWithExceptionWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogError(exception, message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Error, logEvent.Level);
            Assert.Equal(exception, logEvent.Exception);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogErrorWithExceptionWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogError(exception, message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogErrorWithExceptionWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            Exception exception = BogusGenerator.System.Exception();
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogError(exception, message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Error, logEvent.Level);
            Assert.Equal(exception, logEvent.Exception);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogErrorWithExceptionWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            Exception exception = BogusGenerator.System.Exception();
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogError(exception, message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogCriticalWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogCritical(message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Fatal, logEvent.Level);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogCriticalWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _filteredOutEvent = LogEventLevel.Fatal;

            // Act
            _logger.LogCritical(message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogCriticalWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogCritical(message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Fatal, logEvent.Level);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogCriticalWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _filteredOutEvent = LogEventLevel.Fatal;

            // Act
            _logger.LogCritical(message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogCriticalWithExceptionWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogCritical(exception, message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Fatal, logEvent.Level);
            Assert.Equal(exception, logEvent.Exception);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogCriticalWithExceptionWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _filteredOutEvent = LogEventLevel.Fatal;

            // Act
            _logger.LogCritical(exception, message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogCriticalWithExceptionWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            Exception exception = BogusGenerator.System.Exception();
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogCritical(exception, message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Fatal, logEvent.Level);
            Assert.Equal(exception, logEvent.Exception);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogCriticalWithExceptionWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            Exception exception = BogusGenerator.System.Exception();
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _filteredOutEvent = LogEventLevel.Fatal;

            // Act
            _logger.LogCritical(exception, message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogWarningWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogWarning(message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Warning, logEvent.Level);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogWarningWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogWarning(message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogWarningWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogWarning(message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Warning, logEvent.Level);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogWarningWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogWarning(message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogWarningWithExceptionWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogWarning(exception, message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Warning, logEvent.Level);
            Assert.Equal(exception, logEvent.Exception);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogWarningWithExceptionWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogWarning(exception, message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogWarningWithExceptionWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            Exception exception = BogusGenerator.System.Exception();
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogWarning(exception, message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Warning, logEvent.Level);
            Assert.Equal(exception, logEvent.Exception);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogWarningWithExceptionWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            Exception exception = BogusGenerator.System.Exception();
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;

            // Act
            _logger.LogWarning(exception, message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogDebugWithoutArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogDebug(message, context);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Debug, logEvent.Level);
            Assert.Equal(message, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogDebugWithoutArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message!";
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Information;

            // Act
            _logger.LogDebug(message, context);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogDebugWithArgs_WithCustomTelemetry_Succeeds()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            _logger.LogDebug(message, context, argument);

            // Assert
            LogEvent logEvent = Assert.Single(_spySink.CurrentLogEmits);
            Assert.Equal(LogEventLevel.Debug, logEvent.Level);
            string expectedMessageResult = string.Format(message.Replace("{Argument}", "\"{0}\""), argument);
            Assert.Equal(expectedMessageResult, logEvent.RenderMessage());
            Assert.Equal(value, Assert.Contains(key, logEvent.Properties).ToDecentString());
        }

        [Fact]
        public void LogDebugWithArgs_NotActivated_Ignores()
        {
            // Arrange
            var message = "This is a log message with an {Argument}!";
            string argument = BogusGenerator.Commerce.Product();
            string key = BogusGenerator.Commerce.Product();
            string value = BogusGenerator.Commerce.Price();
            var context = new Dictionary<string, object> { [key] = value };

            _logLevelSwitch.MinimumLevel = LogEventLevel.Information;

            // Act
            _logger.LogDebug(message, context, argument);

            // Assert
            Assert.Empty(_spySink.CurrentLogEmits);
        }

        [Fact]
        public void LogInformationWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogInformation(message: null, context));
        }

        [Fact]
        public void LogInformationWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogInformation(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogWarningWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(message: null, context));
        }

        [Fact]
        public void LogWarningWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogDebugWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDebug(message: null, context));
        }

        [Fact]
        public void LogDebugWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDebug(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogTraceWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogTrace(message: null, context));
        }

        [Fact]
        public void LogTraceWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogTrace(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogErrorWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(message: null, context));
        }

        [Fact]
        public void LogErrorWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogErrorWithExceptionWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(message: null, context));
        }

        [Fact]
        public void LogErrorWithExceptionWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogErrorWithExceptionWithoutArgs_WithoutException_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(exception: null, message, context));
        }

        [Fact]
        public void LogErrorWithExceptionWithArgs_WithoutException_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(exception: null, message, context, "This is an argument value"));
        }

        [Fact]
        public void LogErrorWithExceptionWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(exception, message, context: null));
        }

        [Fact]
        public void LogErrorWithExceptionWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(exception, message, context: null, "This is an argument value"));
        }

         [Fact]
        public void LogCriticalWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(message: null, context));
        }

        [Fact]
        public void LogCriticalWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogCriticalWithExceptionWithoutArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(message: null, context));
        }

        [Fact]
        public void LogCriticalWithExceptionWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogCriticalWithExceptionWithoutArgs_WithoutException_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(exception: null, message, context));
        }

        [Fact]
        public void LogCriticalWithExceptionWithArgs_WithoutException_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(exception: null, message, context, "This is an argument value"));
        }

        [Fact]
        public void LogCriticalWithExceptionWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(exception, message, context: null));
        }

        [Fact]
        public void LogCriticalWithExceptionWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCritical(exception, message, context: null, "This is an argument value"));
        }

        [Fact]
        public void LogInformationWithoutArgs_Without_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogInformation(message, context: null));
        }

        [Fact]
        public void LogInformationWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message with an {Argument}!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogInformation(message, context: null, "This is an argument value"));
        }

        [Fact]
        public void LogWarningWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(message, context: null));
        }

        [Fact]
        public void LogWarningWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message with an {Argument}!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(message, context: null, "This is an argument value"));
        }

          [Fact]
        public void LogWarningWithExceptionWithArgs_WithoutMessage_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(message: null, context, "This is an argument value"));
        }

        [Fact]
        public void LogWarningWithExceptionWithoutArgs_WithoutException_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(exception: null, message, context));
        }

        [Fact]
        public void LogWarningWithExceptionWithArgs_WithoutException_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            var context = new Dictionary<string, object>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(exception: null, message, context, "This is an argument value"));
        }

        [Fact]
        public void LogWarningWithExceptionWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(exception, message, context: null));
        }

        [Fact]
        public void LogWarningWithExceptionWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var message = "This is a log message!";
            Exception exception = BogusGenerator.System.Exception();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogWarning(exception, message, context: null, "This is an argument value"));
        }

        [Fact]
        public void LogDebugWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDebug(message, context: null));
        }

        [Fact]
        public void LogDebugWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message with an {Argument}!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDebug(message, context: null, "This is an argument value"));
        }

        [Fact]
        public void LogTraceWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogTrace(message, context: null));
        }

        [Fact]
        public void LogTraceWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message with an {Argument}!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogTrace(message, context: null, "This is an argument value"));
        }

        [Fact]
        public void LogErrorWithoutArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(message, context: null));
        }

        [Fact]
        public void LogErrorWithArgs_WithoutContext_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string message = "This is a log message with an {Argument}!";

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogError(message, context: null, "This is an argument value"));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _loggerFactory?.Dispose();
        }
    }
}
