using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class AzureBlobStorageDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
                () => logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, measurement: (DurationMeasurement)null));
        }
    }
}
