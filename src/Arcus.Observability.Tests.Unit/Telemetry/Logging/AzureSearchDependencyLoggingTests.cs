using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class AzureSearchDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration();
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration();
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
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
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement: (DurationMeasurement)null));
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
                () => logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, measurement: (DurationMeasurement)null, dependencyId));
        }
    }
}
