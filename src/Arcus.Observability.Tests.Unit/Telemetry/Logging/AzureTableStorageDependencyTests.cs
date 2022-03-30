using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class AzureTableStorageDependencyTests
    {
        private readonly Faker _bogusGenerator = new Faker();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement: (DurationMeasurement)null));
        }
    }
}
