using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class EventHubsDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogEventHubsDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            string namespaceName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(namespaceName, logMessage);
            Assert.Contains(eventHubName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = eventHubName;
            Assert.Contains("Azure Event Hubs " + dependencyName, logMessage);
        }

        [Fact]
        public void LogEventHubsDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            string namespaceName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogEventHubsDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            string namespaceName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(namespaceName, logMessage);
            Assert.Contains(eventHubName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = eventHubName;
            Assert.Contains("Azure Event Hubs " + dependencyName, logMessage);
        }

        [Fact]
        public void LogEventHubsDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = _bogusGenerator.Lorem.Word();
            string namespaceName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(eventHubName, dependency.TargetName);
            Assert.Equal("Azure Event Hubs", dependency.DependencyType);
            Assert.Equal(eventHubName, dependency.DependencyName);
            Assert.Equal(namespaceName, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDurationMeasurement_WithoutNamespaceName_Fails(string namespaceName)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDurationMeasurement_WithoutEventHubName_Fails(string eventHubName)
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement));
        }

        [Fact]
        public void LogEventHubsDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = _bogusGenerator.Commerce.ProductName();
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement: (DurationMeasurement)null));
        }
    }
}
