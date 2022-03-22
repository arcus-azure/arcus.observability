using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    // ReSharper disable once InconsistentNaming
    [Trait("Category", "Unit")]
    public class ILoggerExtensionsTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogMetric_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Name.FullName();
            double metricValue = _bogusGenerator.Random.Double();

            // Act
            logger.LogMetric(metricName, metricValue);

            // Assert
            MetricLogEntry metric = logger.GetMessageAsMetric();
            Assert.Equal(metricName, metric.MetricName);
            Assert.Equal(metricValue, metric.MetricValue);
            Assert.Collection(metric.Context, item =>
            {
                Assert.Equal(ContextProperties.General.TelemetryType, item.Key);
                Assert.Equal(TelemetryType.Metrics, item.Value);
            });
        }

        [Fact]
        public void LogMetric_ValidArgumentsWithTimestamp_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Name.FullName();
            double metricValue = _bogusGenerator.Random.Double();
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();

            // Act
            logger.LogMetric(metricName, metricValue, timestamp);

            // Assert
            MetricLogEntry metric = logger.GetMessageAsMetric();
            Assert.Equal(metricName, metric.MetricName);
            Assert.Equal(metricValue, metric.MetricValue);
            Assert.Equal(timestamp.ToString(FormatSpecifiers.InvariantTimestampFormat), metric.Timestamp);
            Assert.Collection(metric.Context, item =>
            {
                Assert.Equal(ContextProperties.General.TelemetryType, item.Key);
                Assert.Equal(TelemetryType.Metrics, item.Value);
            });
        }

        [Fact]
        public void LogMetric_ValidArgumentsWithCustomContext_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Lorem.Word();
            double metricValue = _bogusGenerator.Random.Double();
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            logger.LogMetric(metricName, metricValue, timestamp, context);

            // Assert
            MetricLogEntry metric = logger.GetMessageAsMetric();
            Assert.Equal(metricName, metric.MetricName);
            Assert.Equal(metricValue, metric.MetricValue);
            Assert.Equal(timestamp.ToString(FormatSpecifiers.InvariantTimestampFormat), metric.Timestamp);
            Assert.Collection(metric.Context,
                item =>
                {
                    Assert.Equal(key, item.Key);
                    Assert.Equal(value, item.Value);
                },
                item =>
                {
                    Assert.Equal(ContextProperties.General.TelemetryType, item.Key);
                    Assert.Equal(TelemetryType.Metrics, item.Value);
                });
        }

        [Fact]
        public void LogMetric_NoMetricNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = null;
            double metricValue = _bogusGenerator.Random.Double();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogMetric(metricName, metricValue));
        }

        [Fact]
        public void LogEvent_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string eventName = _bogusGenerator.Name.FullName();

            // Act
            logger.LogEvent(eventName);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Events.ToString(), logMessage);
            Assert.Contains(eventName, logMessage);
        }

        [Fact]
        public void LogEvent_NoEventNameSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string eventName = null;

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogEvent(eventName));
        }

        [Fact]
        public void LogDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
        }

        [Fact]
        public void LogDependency_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(dependencyName, logMessage);
        }

        [Fact]
        public void LogDependency_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration));
        }
        
        [Fact]
        public void LogDependency_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration));
        }
        
        [Fact]
        public void LogDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
        }

        [Fact]
        public void LogDependencyWithDependencyDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurement_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(dependencyName, logMessage);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(dependencyName, logMessage);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogDependencyWithDurationMeasurement_WithoutDependencyType_Fails(string dependencyType)
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData: null, isSuccessful, dependencyName, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string dependencyType = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithTargetName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(targetName, dependency.TargetName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutDependencyType_Fails(string dependencyType)
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData: null, targetName, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurement_WithoutDependencyMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement: (DependencyMeasurement) null));
        }

        [Fact]
        public void LogDependencyWithDependencyDurationMeasurement_WithoutDependencyMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurement_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyWithDependencyDurationMeasurement_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurement_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyWithDependencyDurationMeasurement_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyTarget_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
        }

        [Fact]
        public void LogDependencyTarget_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(dependencyName, logMessage);
        }

        [Fact]
        public void LogDependencyTarget_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyTarget_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyTarget_WithoutTarget_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyTarget_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurementTarget_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurementTarget_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(dependencyName, logMessage);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(dependencyType, logMessage);
            Assert.Contains(dependencyData, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(dependencyName, logMessage);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType: null, dependencyData, targetName, isSuccessful, dependencyName, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData: null, targetName, isSuccessful, dependencyName, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutTargetName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName: null, isSuccessful, dependencyName, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyName, dependency.DependencyName);
            Assert.Null(dependency.TargetName);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName: null, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Null(dependency.DependencyName);
            Assert.Equal(targetName, dependency.TargetName);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Random.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyMeasurement_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyDurationMeasurement_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyMeasurement_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyDurationMeasurement_WithoutDependencyData_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyMeasurement_WithoutDependencyMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: (DependencyMeasurement) null));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyDurationMeasurement_WithoutDependencyMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutSecretName_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: TimeSpan.FromSeconds(5), "dependency ID"));
        }

        [Fact]
        public void LogAzureKeyVaultDependency_WithIdWithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            TimeSpan duration = GeneratePositiveDuration().Negate();
            
            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", "MySecret", isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration, "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5)));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutSecretName_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: TimeSpan.FromSeconds(value: 5)));
        }

        [Fact]
        public void LogAzureKeyVaultDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            TimeSpan duration = GeneratePositiveDuration().Negate();
            
            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", "MySecret", isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: duration));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDependencyMeasurement_WithIdWithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DependencyMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutSecretNameDependencyMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DependencyMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDependencyMeasurement_WithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DependencyMeasurement.Start()));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutSecretNameDependencyMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DependencyMeasurement.Start()));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDurationMeasurement_WithIdWithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DurationMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutSecretNameDurationMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DurationMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDurationMeasurement_WithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DurationMeasurement.Start()));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutSecretNameDurationMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DurationMeasurement.Start()));
        }

        [Fact]
        public void LogAzureSearchDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(searchServiceName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = searchServiceName;
            Assert.Contains("Azure Search " + dependencyName, logMessage);
        }

        [Fact]
        public void LogAzureSearchDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogAzureSearchDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(searchServiceName, dependency.DependencyName);
            Assert.Equal(operationName, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal("Azure Search", dependency.DependencyType);
            Assert.Equal(searchServiceName, dependency.TargetName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureSearchDependencyWithDependencyId_WithoutSearchServiceName_Fails(string searchServiceName)
        {
            // Arrange
            var logger = new TestLogger();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = GeneratePositiveDuration();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureSearchDependencyWithDependencyId_WithoutOperationName_Fails(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = GeneratePositiveDuration();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogAzureSearchDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogAzureSearchDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DependencyMeasurement measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(searchServiceName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = searchServiceName;
            Assert.Contains("Azure Search " + dependencyName, logMessage);
        }

        [Fact]
        public void LogAzureSearchDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DurationMeasurement measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(searchServiceName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = searchServiceName;
            Assert.Contains("Azure Search " + dependencyName, logMessage);
        }

        [Fact]
        public void LogAzureSearchDependencyWithDependencyIdWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyId = _bogusGenerator.Lorem.Word();
            DurationMeasurement measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(searchServiceName, dependency.DependencyName);
            Assert.Equal(operationName, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(measurement.Elapsed, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal("Azure Search", dependency.DependencyType);
            Assert.Equal(searchServiceName, dependency.TargetName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureSearchDependencyWithDurationMeasurement_WithoutSearchServiceName_Fails(string searchServiceName)
        {
            // Arrange
            var logger = new TestLogger();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DurationMeasurement measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureSearchDependencyWithDependencyIdWithDurationMeasurement_WithoutSearchServiceName_Fails(string searchServiceName)
        {
            // Arrange
            var logger = new TestLogger();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DurationMeasurement measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureSearchDependencyWithDurationMeasurement_WithoutOperationName_Fails(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DurationMeasurement measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureSearchDependencyWithDependencyIdWithDurationMeasurement_WithoutOperationName_Fails(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DurationMeasurement measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogAzureSearchDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.ProductName();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogAzureSearchDependencyWithDependencyIdWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string searchServiceName = _bogusGenerator.Commerce.ProductName();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement: (DurationMeasurement) null, dependencyId));
        }

        [Fact]
        public void LogServiceBusDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            var startTime = DateTimeOffset.UtcNow;

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, entityType);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(entityType.ToString(), logMessage);
            Assert.Contains(entityName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            string dependencyName = entityName;
            Assert.Contains("Azure Service Bus " + dependencyName, logMessage);
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, dependencyId, entityType);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(entityName, dependency.TargetName);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(entityType.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, dependencyId, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Topic;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, measurement, entityType);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(entityType.ToString(), logMessage);
            Assert.Contains(entityName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            string dependencyName = entityName;
            Assert.Contains("Azure Service Bus " + dependencyName, logMessage);
        }

        [Fact]
        public void LogServiceBusDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, measurement, entityType);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(entityName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(entityName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(entityType.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyIdWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, measurement, dependencyId, entityType);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(entityName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(entityName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(entityType.ToString(), entityTypeItem.Value);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusDependencyWithDurationMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement, entityType));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusDependencyWithDependencyIdWithDurationMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement, dependencyId, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Commerce.Product();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement: (DurationMeasurement) null, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyIdWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Commerce.Product();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement: (DurationMeasurement) null, dependencyId, entityType));
        }

        [Fact]
        public void LogServiceBusQueueDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            var startTime = DateTimeOffset.UtcNow;

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(ServiceBusEntityType.Queue.ToString(), logMessage);
            Assert.Contains(queueName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            string dependencyName = queueName;
            Assert.Contains("Azure Service Bus " + dependencyName, logMessage);
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            var startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(queueName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(queueName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Queue.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusQueueDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(ServiceBusEntityType.Queue.ToString(), logMessage);
            Assert.Contains(queueName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            string dependencyName = queueName;
            Assert.Contains("Azure Service Bus " + dependencyName, logMessage);
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(queueName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(queueName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Queue.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyIdWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(queueName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(queueName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Queue.ToString(), entityTypeItem.Value);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueDependencyWithDurationMeasurement_WithoutQueueName_Fails(string queueName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueDependencyWithDependencyIdWithDurationMeasurement_WithoutQueueName_Fails(string queueName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyIdWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement: (DurationMeasurement) null, dependencyId));
        }

        [Fact]
        public void LogServiceBusTopicDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            var startTime = DateTimeOffset.UtcNow;

            // Act
            logger.LogServiceBusTopicDependency(topicName, isSuccessful, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(ServiceBusEntityType.Topic.ToString(), logMessage);
            Assert.Contains(topicName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            string dependencyName = topicName;
            Assert.Contains("Azure Service Bus " + dependencyName, logMessage);
        }
        
        [Fact]
        public void LogServiceBusTopicDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusTopicDependency(topicName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogServiceBusTopicDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(ServiceBusEntityType.Topic.ToString(), logMessage);
            Assert.Contains(topicName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            var dependencyName = topicName;
            Assert.Contains("Azure Service Bus " + dependencyName, logMessage);
        }

        [Fact]
        public void LogServiceBusTopicDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(topicName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(topicName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Topic.ToString(), entityTypeItem.Value);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicDependencyWithDurationMeasurement_WithoutTopicName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement));
        }

        [Fact]
        public void LogServiceBusTopicDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogSqlDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(serverName, logMessage);
            Assert.Contains(databaseName, logMessage);
            Assert.Contains(tableName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{databaseName}/{tableName}";
            Assert.Contains("Sql " + dependencyName, logMessage);
        }

        [Fact]
        public void LogSqlDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogBlobStorageDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string containerName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(containerName, logMessage);
            Assert.Contains(accountName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{accountName}/{containerName}";
            Assert.Contains("Azure blob " + dependencyName, logMessage);
        }
        
        [Fact]
        public void LogBlobStorageDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string containerName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogBlobStorageDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string containerName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(containerName, logMessage);
            Assert.Contains(accountName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{accountName}/{containerName}";
            Assert.Contains("Azure blob " + dependencyName, logMessage);
        }

        [Fact]
        public void LogBlobStorageDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string containerName = _bogusGenerator.Lorem.Word();
            string accountName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal(containerName, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(accountName + "/" + containerName, dependency.DependencyName);
            Assert.Equal("Azure blob", dependency.DependencyType);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogBlobStorageDependencyWithDurationMeasurement_WithoutAccountName_Fails(string accountName)
        {
            // Arrange
            var logger = new TestLogger();
            string containerName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / 
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogBlobStorageDependencyWithDurationMeasurement_WithoutContainerName_Fails(string containerName)
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / 
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, measurement));
        }

        [Fact]
        public void LogBlobStorageDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Finance.AccountName();
            string containerName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            // Act / 
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogTableStorageDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(tableName, logMessage);
            Assert.Contains(accountName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{accountName}/{tableName}";
            Assert.Contains("Azure table " + dependencyName, logMessage);
        }

        [Fact]
        public void LogTableStorageDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogTableStorageDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(tableName, logMessage);
            Assert.Contains(accountName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{accountName}/{tableName}";
            Assert.Contains("Azure table " + dependencyName, logMessage);
        }

        [Fact]
        public void LogTableStorageDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Lorem.Word();
            string accountName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal("Azure table", dependency.DependencyType);
            Assert.Equal(accountName + "/" + tableName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogTableStorageDependencyWithDurationMeasurement_WithoutAccountName_Fails(string accountName)
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogTableStorageDependencyWithDurationMeasurement_WithoutTableName_Fails(string tableName)
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement));
        }

        [Fact]
        public void LogTableStorageDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Commerce.ProductName();
            string tableName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement: (DurationMeasurement) null));
        }

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
            TimeSpan duration = GeneratePositiveDuration().Negate();

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
                () => logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogIotHubDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: isSuccessful, startTime: startTime, duration: duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = iotHubName;
            Assert.Contains("Azure IoT Hub " + dependencyName, logMessage);
        }

        [Fact]
        public void LogIoTHubDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: isSuccessful, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogIotHubDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: isSuccessful, measurement: measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = iotHubName;
            Assert.Contains("Azure IoT Hub " + dependencyName, logMessage);
        }

        [Fact]
        public void LogIotHubDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(iotHubName, dependency.TargetName);
            Assert.Equal("Azure IoT Hub", dependency.DependencyType);
            Assert.Equal(iotHubName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var iotHubName = "acme.azure-devices.net";
            var iotHubConnectionString = $"HostName={iotHubName};SharedAccessKeyName=AllAccessKey;DeviceId=fake;SharedAccessKey=dGVzdFN0cmluZzE=";
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(iotHubName, dependency.TargetName);
            Assert.Equal("Azure IoT Hub", dependency.DependencyType);
            Assert.Equal(iotHubName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyWithDurationMeasurement_WithoutIoTHubName_Fails(string iotHubName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement));
        }

        [Fact]
        public void LogIotHubDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string iotHubName = _bogusGenerator.Commerce.ProductName();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurement_WithoutConnectionString_Fails(string iotHubConnectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement));
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string iotHubConnectionString = _bogusGenerator.Commerce.ProductName();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogIotHubConnectionStringDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = _bogusGenerator.Internet.Ip();
            string sharedAccessKey = _bogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful: isSuccessful, startTime: startTime, duration: duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = iotHubName;
            Assert.Contains("Azure IoT Hub " + dependencyName, logMessage);
        }
        
        [Fact]
        public void LogIoTHubConnectionStringDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = _bogusGenerator.Internet.Ip();
            string sharedAccessKey = _bogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful: isSuccessful, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = _bogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = _bogusGenerator.Internet.Ip();
            string sharedAccessKey = _bogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful: isSuccessful, measurement: measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(iotHubName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = iotHubName;
            Assert.Contains("Azure IoT Hub " + dependencyName, logMessage);
        }

        [Fact]
        public void LogCosmosSqlDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(container, logMessage);
            Assert.Contains(database, logMessage);
            Assert.Contains(accountName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{database}/{container}";
            Assert.Contains("Azure DocumentDB " + dependencyName, logMessage);
        }

        [Fact]
        public void LogCosmosSqlDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(container, logMessage);
            Assert.Contains(database, logMessage);
            Assert.Contains(accountName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{database}/{container}";
            Assert.Contains("Azure DocumentDB " + dependencyName, logMessage);
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = _bogusGenerator.Lorem.Word();
            string database = _bogusGenerator.Lorem.Word();
            string accountName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal("Azure DocumentDB", dependency.DependencyType);
            Assert.Equal(database + "/" + container, dependency.DependencyName);
            Assert.Equal(database + "/" + container, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutAccountName_Fails(string accountName)
        {
            // Arrange
            var logger = new TestLogger();
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutDatabase_Fails(string database)
        {
            // Arrange
            var logger = new TestLogger();
            string container = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutContainer_Fails(string container)
        {
            // Arrange
            var logger = new TestLogger();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement));
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string database = _bogusGenerator.Commerce.ProductName();
            string container = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogSqlDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DependencyMeasurement.Start(operationName);
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogSqlDependency(serverName, databaseName, tableName, isSuccessful, measurement);

            // Assert

            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(serverName, logMessage);
            Assert.Contains(databaseName, logMessage);
            Assert.Contains(tableName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{databaseName}/{tableName}";
            Assert.Contains("Sql " + dependencyName, logMessage);
        }

        [Fact]
        public void LogSqlDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Lorem.Word();
            string databaseName = _bogusGenerator.Lorem.Word();
            string tableName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Equal("Sql", dependency.DependencyType);
            Assert.Equal(databaseName + "/" + tableName, dependency.DependencyName);
            Assert.Equal(operationName, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutServerName_Fails(string serverName)
        {
            // Arrange
            var logger = new TestLogger();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutDatabaseName_Fails(string databaseName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutTableName_Fails(string tableName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutOperation_Fails(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement));
        }

        [Fact]
        public void LogSqlDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogSqlDependencyConnectionString_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogSqlDependency(connectionString, tableName, operationName, startTime, duration, isSuccessful);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(serverName, logMessage);
            Assert.Contains(databaseName, logMessage);
            Assert.Contains(tableName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{databaseName}/{tableName}";
            Assert.Contains("Sql " + dependencyName, logMessage);
        }

        [Fact]
        public void LogSqlDependencyConnectionStringWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Lorem.Word();
            string databaseName = _bogusGenerator.Lorem.Word();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string tableName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogSqlDependency(connectionString, tableName, operationName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Equal("Sql", dependency.DependencyType);
            Assert.Equal(databaseName + "/" + tableName, dependency.DependencyName);
            Assert.Equal(operationName, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyConnectionStringWithDurationMeasurement_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(connectionString, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyConnectionStringWithDurationMeasurement_WithoutTableName_Fails(string tableName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(connectionString, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyConnectionStringWithDurationMeasurement_WithoutOperationName_Fails(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string tableName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(connectionString, tableName, operationName, isSuccessful, measurement));
        }

        [Fact]
        public void LogSqlDependencyConnectionStringWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(connectionString, tableName, operationName, isSuccessful, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogSqlDependencyConnectionString_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

                // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogSqlDependency(connectionString, tableName, operationName, startTime, duration, isSuccessful));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LogSqlDependencyConnectionString_EmptyConnectionString_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => logger.LogSqlDependency(connectionString, tableName, operationName, startTime, duration, isSuccessful));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LogSqlDependencyWithDependencyMeasurementConnectionString_EmptyConnectionString_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            using (var measurement = DependencyMeasurement.Start(operationName))
            {
                // Act / Assert
                Assert.Throws<ArgumentException>(
                    () => logger.LogSqlDependency(connectionString, tableName, operationName, measurement, isSuccessful));
            }
        }

        [Fact]
        public void LogSqlDependency_NoServerNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = null;
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependency_NoDatabaseNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = null;
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependency_NoTableNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = null;
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependency_NoOperationNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = null;
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithDependencyMeasurementConnectionString_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DependencyMeasurement.Start(operationName);
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogSqlDependency(connectionString, tableName, operationName, measurement, isSuccessful);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(serverName, logMessage);
            Assert.Contains(databaseName, logMessage);
            Assert.Contains(tableName, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{databaseName}/{tableName}";
            Assert.Contains("Sql " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.Url());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogHttpDependency(request, statusCode, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri.Host, logMessage);
            Assert.Contains(request.RequestUri.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int) statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            var isSuccessful = (int) statusCode >= 200 && (int) statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.Url());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogHttpDependency_NoRequestWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            HttpRequestMessage request = null;
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogHttpDependency(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogHttpDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogHttpDependency(request, statusCode, measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri.Host, logMessage);
            Assert.Contains(request.RequestUri.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int) statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            var isSuccessful = (int) statusCode >= 200 && (int) statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogHttpDependency(request, statusCode, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri?.Host, logMessage);
            Assert.Contains(request.RequestUri?.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            bool isSuccessful = (int) statusCode >= 200 && (int) statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri?.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: null, statusCode, measurement));
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: (DurationMeasurement) null));
        }

        [Fact]
        public void LogRequest_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            var host = _bogusGenerator.Lorem.Word();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogRequest(mockRequest.Object, mockResponse.Object, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(statusCode.ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequest_ValidArgumentsIncludingResponseStatusCode_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            var host = _bogusGenerator.Lorem.Word();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogRequest(mockRequest.Object, statusCode, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(statusCode.ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            var host = _bogusGenerator.Lorem.Word();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(mockRequest.Object, mockResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithSchemeWithWhitespaceWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("hostname"));
            saboteurRequest.Setup(r => r.Scheme).Returns("scheme with spaces");
            var stubResponse = new Mock<HttpResponse>();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, stubResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithoutHostWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString());
            var stubResponse = new Mock<HttpResponse>();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, stubResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithHostWithWhitespaceWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("host with spaces"));
            var stubResponse = new Mock<HttpResponse>();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, stubResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_NoRequestWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpRequest request = null;
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogRequest(request, mockResponse.Object, duration));
        }
        
        [Fact]
        public void LogRequest_OutsideResponseStatusCodeRange_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom(
                _bogusGenerator.Random.Int(max: -1), 
                _bogusGenerator.Random.Int(min: 1000));
            HttpRequest request = null;
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogRequest(request, mockResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithSchemeWithWhitespaceWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("hostname"));
            saboteurRequest.Setup(r => r.Scheme).Returns("scheme with spaces");
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_RequestWithoutHostWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString());
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_RequestWithHostWithWhitespaceWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("host with spaces"));
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_NoRequestWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpRequest request = null;
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogRequest(request, statusCode, duration));
        }

        [Fact]
        public void LogRequest_OutsideResponseStatusCodeRangeWhenPassingResponseStatusCode_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new Mock<HttpRequest>();
            var statusCode = _bogusGenerator.PickRandom(
                _bogusGenerator.Random.Int(max: -1), 
                _bogusGenerator.Random.Int(min: 1000));
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(request.Object, statusCode, duration));
        }
        
        [Fact]
        public void LogRequest_NoResponseWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Name.FirstName()}";
            var host = _bogusGenerator.Name.FirstName();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            HttpResponse response = null;
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogRequest(mockRequest.Object, response, duration));
        }

        [Fact]
        public void LogRequestMessage_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            var response = new HttpResponseMessage(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogRequest(request, response, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequestMessage_ValidArgumentsIncludingResponseStatusCode_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            var duration = _bogusGenerator.Date.Timespan();

            // Act;
            logger.LogRequest(request, statusCode, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequestMessage_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            TimeSpan duration = GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, duration));
        }
        
        [Fact]
        public void LogRequestMessage_OutsideResponseStatusCodeRange_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode) _bogusGenerator.PickRandom(
                _bogusGenerator.Random.Int(max: -1), 
                _bogusGenerator.Random.Int(min: 1000));
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, duration));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subcriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subcriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutTopicName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutTopicName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusTopicRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffix_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);
            
            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutEntityName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutEntityName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Fact]
        public void LogServiceBusTopicRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffix_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusQueueRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffix_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, duration, startTime, context);
            
            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequest_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context);
            
            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequest_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequest_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Fact]
        public void LogServiceBusQueueRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffix_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSufix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSufix, entityName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusRequestWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusRequestWithSuffixWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Fact]
        public void LogServiceBusRequestWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement: null, entityType, context));
        }

        [Fact]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement: null, entityType, context));
        }

        [Fact]
        public void LogServiceBusRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusRequestWithSuffix_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entityType, context);
            
            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequest_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entityType, context);
            
            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequest_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequest_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Fact]
        public void LogServiceBusRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, negativeDuration, startTime, entryType, context));
        }

        [Fact]
        public void LogServiceBusRequestWithSuffix_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            
            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, negativeDuration, startTime, entryType, context));
        }

        [Fact]
        public void LogSecurityEvent_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            const string message = "something was invalidated wrong";

            // Act
            logger.LogSecurityEvent(message);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Events.ToString(), logMessage);
            Assert.Contains(message, logMessage);
            Assert.Contains("[EventType, Security]", logMessage);
        }

        [Fact]
        public void LogSecurityEvent_ValidArgumentsWithContext_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            const string message = "something was invalidated wrong";
            var telemetryContext = new Dictionary<string, object>
            {
                ["Property"] = "something was wrong with this Property"
            };

            // Act
            logger.LogSecurityEvent(message, telemetryContext);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Events.ToString(), logMessage);
            Assert.Contains(message, logMessage);
            Assert.Contains("[EventType, Security]", logMessage);
            Assert.Contains("[Property, something was wrong with this Property]", logMessage);
        }

        [Fact]
        public void LogSecurityEvent_WithNoEventName_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string eventName = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogSecurityEvent(eventName));
        }

        private TimeSpan GeneratePositiveDuration()
        {
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            if (duration < TimeSpan.Zero)
            {
                return duration.Negate();
            }

            return duration;
        }
    }
}
