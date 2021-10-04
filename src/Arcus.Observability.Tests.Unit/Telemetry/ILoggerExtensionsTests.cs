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
                Assert.Equal(nameof(TelemetryType), item.Key);
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
                Assert.Equal(nameof(TelemetryType), item.Key);
                Assert.Equal(TelemetryType.Metrics, item.Value);
            });
        }

        [Fact]
        public void LogMetric_ValidArgumentsWithCustomContext_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Name.FullName();
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
                    Assert.Equal(nameof(TelemetryType), item.Key);
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
        public void LogDependencyWithDependencyMeasurement_WithoutDependencyMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, isSuccessful, measurement: null));
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
        public void LogDependencyTargetWithDependencyMeasurement_WithoutDependencyMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string dependencyType = _bogusGenerator.Lorem.Word();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, measurement: null));
        }

        [Theory]
        [ClassData(typeof(ValidAzureKeyVaultSecretNames))]
        public void LogAzureKeyVaultDependency_WithValidSecretName_Succeeds(string secretName)
        {
            // Arrange
            var logger = new TestLogger();
            var vaultUri = "https://myvault.vault.azure.net";
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(vaultUri, logMessage);
            Assert.Contains(secretName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = vaultUri;
            Assert.Contains("Azure key vault " + dependencyName, logMessage);
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

        [Theory]
        [ClassData(typeof(InvalidAzureKeyVaultSecretNames))]
        public void LogAzureKeyVaultDependency_WithInvalidSecretName_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.Throws<FormatException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: TimeSpan.FromSeconds(value: 5)));
        }

        [Fact]
        public void LogAzureKeyVaultDependency_WithoutMatchingVaultUri_Fails()
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.Throws<UriFormatException>(
                () => logger.LogAzureKeyVaultDependency("https://vault-without-vault.azure.net-suffix", "MySecret", isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: TimeSpan.FromSeconds(5)));
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
        [ClassData(typeof(ValidAzureKeyVaultSecretNames))]
        public void LogAzureKeyVaultDependency_WithValidSecretNameDependencyMeasurement_Succeeds(string secretName)
        {
            // Arrange
            var logger = new TestLogger();
            var vaultUri = "https://myvault.vault.azure.net";
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DependencyMeasurement measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(vaultUri, logMessage);
            Assert.Contains(secretName, logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            string dependencyName = vaultUri;
            Assert.Contains("Azure key vault " + dependencyName, logMessage);
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
        [ClassData(typeof(InvalidAzureKeyVaultSecretNames))]
        public void LogAzureKeyVaultDependency_WithInvalidSecretNameDependencyMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.Throws<FormatException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DependencyMeasurement.Start()));
        }

        [Fact]
        public void LogAzureKeyVaultDependencyDependencyMeasurement_WithoutMatchingVaultUri_Fails()
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.Throws<UriFormatException>(
                () => logger.LogAzureKeyVaultDependency("https://vault-without-vault.azure.net-suffix", "MySecret", isSuccessful: true, measurement: DependencyMeasurement.Start()));
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
        public void LogAzureSearchDependencyWithDependencyMeasurement_WithNegativeDuration_Fails()
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
