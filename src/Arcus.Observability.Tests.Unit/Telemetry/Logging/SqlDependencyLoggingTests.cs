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
    public class SqlDependencyLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogSqlDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

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
        public void LogSqlDependencyWithSqlCommand_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Lorem.Word();
            string databaseName = BogusGenerator.Lorem.Word();
            string sqlCommand = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand, isSuccessful, startTime, duration, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Equal(databaseName, dependency.DependencyName);
            Assert.Equal(sqlCommand, dependency.DependencyData);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandAndOperationName_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Lorem.Word();
            string databaseName = BogusGenerator.Lorem.Word();
            string sqlCommand = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            string dependencyId = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Contains(databaseName, dependency.DependencyName);
            Assert.Contains(operationName, dependency.DependencyName);
            Assert.Equal(sqlCommand, dependency.DependencyData);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Lorem.Word();
            string databaseName = BogusGenerator.Lorem.Word();
            string sqlCommand = BogusGenerator.Lorem.Word();
            string dependencyId = BogusGenerator.Lorem.Word(); 
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Equal(databaseName, dependency.DependencyName);
            Assert.Equal(sqlCommand, dependency.DependencyData);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Fact]
        public void LogSqlDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandAndOperationName_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

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
            string serverName = BogusGenerator.Lorem.Word();
            string databaseName = BogusGenerator.Lorem.Word();
            string tableName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();

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

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Lorem.Word();
            string databaseName = BogusGenerator.Lorem.Word();
            string sqlCommand = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Equal(databaseName, dependency.DependencyName);
            Assert.Equal(sqlCommand, dependency.DependencyData);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Lorem.Word();
            string databaseName = BogusGenerator.Lorem.Word();
            string sqlCommand = BogusGenerator.Lorem.Word();
            string dependencyId = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(serverName, dependency.TargetName);
            Assert.Equal(databaseName, dependency.DependencyName);
            Assert.Equal(sqlCommand, dependency.DependencyData);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutServerName_Fails(string serverName)
        {
            // Arrange
            var logger = new TestLogger();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurement_WithoutServerName_Fails(string serverName)
        {
            // Arrange
            var logger = new TestLogger();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurementWithDependencyId_WithoutServerName_Fails(string serverName)
        {
            // Arrange
            var logger = new TestLogger();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithSqlCommandAndOperationNameWithDurationMeasurement_WithoutServerName_Fails(string serverName)
        {
            // Arrange
            var logger = new TestLogger();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutDatabaseName_Fails(string databaseName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurement_WithoutDatabaseName_Fails(string databaseName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurementWithDependencyId_WithoutDatabaseName_Fails(string databaseName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithSqlCommandAndOperationNameWithDurationMeasurementWithDependencyId_WithoutDatabaseName_Fails(string databaseName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, measurement, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogSqlDependencyWithDurationMeasurement_WithoutTableName_Fails(string tableName)
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

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
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

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
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement: null));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandAndOperationNameWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogSqlDependency_NoServerNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = null;
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommand_NoServerNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = null;
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, sqlCommand, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDependencyId_NoServerNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = null;
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandAndOperationNameWithDependencyId_NoServerNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = null;
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogSqlDependency_NoDatabaseNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = null;
            string tableName = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommand_NoDatabaseNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = null;
            string sqlCommand = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, sqlCommand, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandWithDependencyId_NoDatabaseNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = null;
            string sqlCommand = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithSqlCommandAndOperationNameWithDependencyId_NoDatabaseNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = null;
            string sqlCommand = BogusGenerator.Name.FullName();
            string operationName = BogusGenerator.Name.FullName();
            string dependencyId = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(
                () => logger.LogSqlDependency(serverName, databaseName, sqlCommand: sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogSqlDependency_NoTableNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = null;
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependency_NoOperationNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string tableName = BogusGenerator.Name.FullName();
            string operationName = null;
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogSqlDependencyWithDurationMeasurementConnectionString_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();

            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, measurement, dependencyId);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(serverName, logMessage);
            Assert.Contains(databaseName, logMessage);
            Assert.Contains(sqlCommand, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{databaseName}/{operationName}";
            Assert.Contains("Sql " + dependencyName, logMessage);
        }

        [Fact]
        public void LogSqlDependencyWithDConnectionString_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();

            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act
            logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(serverName, logMessage);
            Assert.Contains(databaseName, logMessage);
            Assert.Contains(sqlCommand, logMessage);
            Assert.Contains(operationName, logMessage);
            Assert.Contains(isSuccessful.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = $"{databaseName}/{operationName}";
            Assert.Contains("Sql " + dependencyName, logMessage);
        }

        [Fact]
        public void LogSqlDependencyWithConnectionString_WithoutConnectionString_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();

            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependencyWithConnectionString(connectionString: null, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithConnectionStringWithDurationMeasurement_WithoutConnectionString_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependencyWithConnectionString(connectionString: null, sqlCommand, operationName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithConnectionStringWithDurationMeasurement_WithoutDurationMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogSqlDependencyWithDurationMeasurement_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();
            using var measurement = DurationMeasurement.Start();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, measurement, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogSqlDependencyWithStartDuration_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();
            var startTime = BogusGenerator.Date.RecentOffset();
            var duration = BogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogSqlDependencyWithConnectionStringWithDurationMeasurement_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();
            using var measurement = DurationMeasurement.Start();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, measurement, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogSqlDependencyWithConnectionStringWithStartDuration_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string serverName = BogusGenerator.Name.FullName();
            string databaseName = BogusGenerator.Name.FullName();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string sqlCommand = "GET something FROM something";
            string operationName = BogusGenerator.Name.FullName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            var dependencyId = BogusGenerator.Random.Guid().ToString();
            var startTime = BogusGenerator.Date.RecentOffset();
            var duration = BogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
