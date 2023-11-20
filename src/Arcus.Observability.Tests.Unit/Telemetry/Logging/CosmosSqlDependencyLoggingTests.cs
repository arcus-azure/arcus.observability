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
    public class CosmosSqlDependencyLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogCosmosSqlDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string database = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

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
        public void LogCosmosSqlDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Lorem.Word();
            string database = BogusGenerator.Lorem.Word();
            string accountName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal("Azure DocumentDB", dependency.DependencyType);
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal($"{database}/{container}", dependency.DependencyName);
            Assert.Equal($"{database}/{container}", dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Fact]
        public void LogCosmosSqlDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string database = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string database = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Lorem.Word();
            string database = BogusGenerator.Lorem.Word();
            string accountName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();

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

        [Fact]
        public void LogCosmosSqlDependencyWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Lorem.Word();
            string database = BogusGenerator.Lorem.Word();
            string accountName = BogusGenerator.Lorem.Word();
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
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(accountName, dependency.TargetName);
            Assert.Equal("Azure DocumentDB", dependency.DependencyType);
            Assert.Equal(database + "/" + container, dependency.DependencyName);
            Assert.Equal(database + "/" + container, dependency.DependencyData);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutAccountName_Fails(string accountName)
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string database = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurementWithDependencyId_WithoutAccountName_Fails(string accountName)
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string database = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutDatabase_Fails(string database)
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurementWithDependencyId_WithoutDatabase_Fails(string database)
        {
            // Arrange
            var logger = new TestLogger();
            string container = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutContainer_Fails(string container)
        {
            // Arrange
            var logger = new TestLogger();
            string database = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogCosmosSqlDependencyWithDurationMeasurementWithDependencyId_WithoutContainer_Fails(string container)
        {
            // Arrange
            var logger = new TestLogger();
            string database = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string database = BogusGenerator.Commerce.ProductName();
            string container = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogCosmosSqlDependencyWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string database = BogusGenerator.Commerce.ProductName();
            string container = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogCosmosSqlDependency_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string database = BogusGenerator.Commerce.ProductName();
            string container = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var startTime = BogusGenerator.Date.RecentOffset();
            var duration = BogusGenerator.Date.Timespan();
            var dependencyId = BogusGenerator.Random.Guid().ToString();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
