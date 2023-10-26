using System;
using System.Collections.Generic;
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
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogEventHubsDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            string namespaceName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

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
        public void LogEventHubsDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Lorem.Word();
            string namespaceName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal("Azure Event Hubs", dependency.DependencyType);
            Assert.Equal(eventHubName, dependency.DependencyName);
            Assert.Equal(namespaceName, dependency.DependencyData);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependency_WithoutNamespaceName_Fails(string namespaceName)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDependencyId_WithoutNamespaceName_Fails(string namespaceName)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependency_WithoutEventHubName_Fails(string eventHubName)
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDependencyId_WithoutEventHubName_Fails(string eventHubName)
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogEventHubsDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            string namespaceName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogEventHubsDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            string namespaceName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogEventHubsDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            string namespaceName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

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
            string eventHubName = BogusGenerator.Lorem.Word();
            string namespaceName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();

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

        [Fact]
        public void LogEventHubsDependencyWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Lorem.Word();
            string namespaceName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = BogusGenerator.Lorem.Word();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(eventHubName, dependency.TargetName);
            Assert.Equal("Azure Event Hubs", dependency.DependencyType);
            Assert.Equal(eventHubName, dependency.DependencyName);
            Assert.Equal(namespaceName, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDurationMeasurement_WithoutNamespaceName_Fails(string namespaceName)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDurationMeasurementWithDependencyId_WithoutNamespaceName_Fails(string namespaceName)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDurationMeasurement_WithoutEventHubName_Fails(string eventHubName)
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEventHubsDependencyWithDurationMeasurementWithDependencyId_WithoutEventHubName_Fails(string eventHubName)
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogEventHubsDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogEventHubsDependencyWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogEventHubsDependency_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string namespaceName = BogusGenerator.Commerce.ProductName();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var startTime = BogusGenerator.Date.RecentOffset();
            var duration = BogusGenerator.Date.Timespan();
            var dependencyId = BogusGenerator.Random.Guid().ToString();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
