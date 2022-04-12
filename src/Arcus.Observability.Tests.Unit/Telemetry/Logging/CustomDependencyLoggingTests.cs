using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class CustomDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
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
        public void LogDependency_ValidArgumentsWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogDependency_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
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
        public void LogDependency_ValidArgumentsWithDependencyNameWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependency_WithoutDependencyTypeWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogDependency_WithoutDependencyDataWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogDependency_WithNegativeDurationWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, dependencyId));
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
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithDependencyId_Succeeds()
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
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
        public void LogDependencyWithDependencyDurationMeasurement_ValidArgumentsWithDependencyId_Succeeds()
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
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogDependencyWithDependencyMeasurement_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
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
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithDependencyNameWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
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
            string dependencyName = _bogusGenerator.Lorem.Word();
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
            string dependencyName = _bogusGenerator.Lorem.Word();
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
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement: (DurationMeasurement)null));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogDependencyWithDurationMeasurement_WithoutDependencyTypeWithDependencyId_Fails(string dependencyType)
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement, dependencyId));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_WithoutDependencyDataWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData: null, isSuccessful, dependencyName, measurement, dependencyId));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_WithoutMeasurementWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string dependencyType = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, measurement: null, dependencyId));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithTargetName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
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

        [Fact]
        public void LogDependencyWithDurationMeasurement_ValidArgumentsWithTargetNameWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(targetName, dependency.TargetName);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutDependencyType_Fails(string dependencyType)
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
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
            string targetName = _bogusGenerator.Lorem.Word();
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
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutDependencyTypeWithDependencyId_Fails(string dependencyType)
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutDependencyDataWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData: null, targetName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementWithTargetName_WithoutMeasurementWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string targetName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: null, dependencyId));
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
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement: (DependencyMeasurement)null));
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
        public void LogDependencyWithDependencyDurationMeasurement_WithoutDependencyMeasurementWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement: null, dependencyId));
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
        public void LogDependencyWithDependencyDurationMeasurement_WithoutDependencyTypeWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement, dependencyId));
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
        public void LogDependencyWithDependencyDurationMeasurement_WithoutDependencyDataWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogDependencyTarget_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
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
        public void LogDependencyTarget_ValidArgumentsWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogDependencyTarget_ValidArgumentsWithDependencyName_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
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
        public void LogDependencyTarget_ValidArgumentsWithDependencyNameWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Database.Type();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyName, dependency.DependencyName);
            Assert.Equal(targetName, dependency.TargetName);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyTarget_WithoutDependencyTypeWithDependency_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogDependencyTarget_WithoutDependencyDataWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogDependencyTarget_WithoutTargetWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = null;
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = _bogusGenerator.Lorem.Word();

                // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogDependencyTarget_WithNegativeDurationWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration, dependencyId));
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
            string dependencyName = _bogusGenerator.Lorem.Word();
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
            string dependencyName = _bogusGenerator.Lorem.Word();
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
        public void LogDependencyWithDurationMeasurementTarget_ValidArgumentsWithDependencyNameWithDependencyId_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyName, dependency.DependencyName);
            Assert.Equal(targetName, dependency.TargetName);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutDependencyType_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType: null, dependencyData, targetName, isSuccessful, dependencyName, measurement));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutDependencyTypeWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType: null, dependencyData, targetName, isSuccessful, dependencyName, measurement, dependencyId));
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
        public void LogDependencyWithDurationMeasurementTarget_WithoutTargetNameWithDependencyId_Succeeds()
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
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName: null, isSuccessful, dependencyName, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyName, dependency.DependencyName);
            Assert.Null(dependency.TargetName);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
        public void LogDependencyWithDurationMeasurementTarget_WithoutDependencyNameWithDependencyId_Succeeds()
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
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName: null, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(dependencyType, dependency.DependencyType);
            Assert.Equal(dependencyData, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Null(dependency.DependencyName);
            Assert.Equal(targetName, dependency.TargetName);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
            string dependencyName = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogDependencyWithDurationMeasurementTarget_WithoutMeasurementWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyName = _bogusGenerator.Lorem.Word();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement: null, dependencyId));
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
        public void LogDependencyTargetWithDependencyDurationMeasurement_WithoutDependencyTypeWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = null;
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement, dependencyId));
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
        public void LogDependencyTargetWithDependencyDurationMeasurement_WithoutDependencyDataWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Name.FullName();
            object dependencyData = null;
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement, dependencyId));
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
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: (DependencyMeasurement)null));
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
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogDependencyTargetWithDependencyDurationMeasurement_WithoutDependencyMeasurementWithDependencyId_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyId = _bogusGenerator.Lorem.Word();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: null, dependencyId));
        }
    }
}
