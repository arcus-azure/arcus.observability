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
    public class IoTHubDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement: (DurationMeasurement)null));
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

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
    }
}
