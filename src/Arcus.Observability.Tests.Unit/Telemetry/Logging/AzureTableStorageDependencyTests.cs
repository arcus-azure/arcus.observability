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
        public void LogTableStorageDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Lorem.Word();
            string accountName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.NotNull(dependency);
            Assert.Equal("Azure table", dependency.DependencyType);
            Assert.StartsWith(accountName, dependency.DependencyName);
            Assert.EndsWith(tableName, dependency.DependencyName);
            Assert.Equal(tableName, dependency.DependencyData);
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
        public void LogTableStorageDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, dependencyId));
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

        [Fact]
        public void LogTableStorageDependencyWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
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
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.NotNull(dependency);
            Assert.Equal("Azure table", dependency.DependencyType);
            Assert.StartsWith(accountName, dependency.DependencyName);
            Assert.EndsWith(tableName, dependency.DependencyName);
            Assert.Equal(tableName, dependency.DependencyData);
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(dependencyId, dependency.DependencyId);
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
        public void LogTableStorageDependencyWithDurationMeasurementDependencyId_WithoutAccountName_Fails(string accountName)
        {
            // Arrange
            var logger = new TestLogger();
            string tableName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement, dependencyId));
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

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogTableStorageDependencyWithDurationMeasurementWithDependencyId_WithoutTableName_Fails(string tableName)
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement, dependencyId));
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

        [Fact]
        public void LogTableStorageDependencyWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Commerce.ProductName();
            string tableName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogTableStorageDependency(accountName, tableName, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogTableStorageDependency_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string accountName = _bogusGenerator.Commerce.ProductName();
            string tableName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string dependencyId = _bogusGenerator.Lorem.Word();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
