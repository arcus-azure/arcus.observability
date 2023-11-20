using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class EventLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogCustomEvent_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventName = _bogusGenerator.Name.FullName();

            // Act
            logger.LogCustomEvent(eventName);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Events.ToString(), logMessage);
            Assert.Contains(eventName, logMessage);
        }

        [Fact]
        public void LogCustomEvent_NoEventNameSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string eventName = null;

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogCustomEvent(eventName));
        }

        [Fact]
        public void LogCustomEvent_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string eventName = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogCustomEvent(eventName, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
