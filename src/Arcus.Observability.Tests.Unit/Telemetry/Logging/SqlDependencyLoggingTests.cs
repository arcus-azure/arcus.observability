using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class SqlDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
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
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement: (DurationMeasurement)null));
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
                () => logger.LogSqlDependency(connectionString, tableName, operationName, isSuccessful, measurement: (DurationMeasurement)null));
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
    }
}
