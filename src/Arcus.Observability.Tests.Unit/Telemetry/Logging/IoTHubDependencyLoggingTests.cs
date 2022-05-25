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
    public class IoTHubDependencyLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogIotHubDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

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
        public void LogIotHubDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(iotHubName, dependency.DependencyName);
            Assert.Null(dependency.DependencyData);
            Assert.Equal(iotHubName, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependency_WithoutIoTHubName_Fails(string iotHubName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, startTime, duration));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyWithDependencyId_WithoutIoTHubName_Fails(string iotHubName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogIoTHubDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: isSuccessful, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogIoTHubDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogIotHubDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            bool isSuccessful = BogusGenerator.Random.Bool();

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
            string iotHubName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();

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
        public void LogIotHubDependencyWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Lorem.Word();
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
            logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(iotHubName, dependency.TargetName);
            Assert.Equal("Azure IoT Hub", dependency.DependencyType);
            Assert.Equal(iotHubName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var iotHubName = "acme.azure-devices.net";
            var iotHubConnectionString = $"HostName={iotHubName};SharedAccessKeyName=AllAccessKey;DeviceId=fake;SharedAccessKey=dGVzdFN0cmluZzE=";
            bool isSuccessful = BogusGenerator.Random.Bool();

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

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var iotHubName = "acme.azure-devices.net";
            var iotHubConnectionString = $"HostName={iotHubName};SharedAccessKeyName=AllAccessKey;DeviceId=fake;SharedAccessKey=dGVzdFN0cmluZzE=";
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
            logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(iotHubName, dependency.TargetName);
            Assert.Equal("Azure IoT Hub", dependency.DependencyType);
            Assert.Equal(iotHubName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyWithDurationMeasurement_WithoutIoTHubName_Fails(string iotHubName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyWithDurationMeasurementWithDependencyId_WithoutIoTHubName_Fails(string iotHubName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogIotHubDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();
            string iotHubName = BogusGenerator.Commerce.ProductName();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogIotHubDependencyWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful, measurement: null, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurement_WithoutConnectionString_Fails(string iotHubConnectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurementWithDependencyId_WithoutConnectionString_Fails(string iotHubConnectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();
            string iotHubConnectionString = BogusGenerator.Commerce.ProductName();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();
            string iotHubConnectionString = BogusGenerator.Commerce.ProductName();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, measurement: null, dependencyId));
        }

        [Fact]
        public void LogIotHubConnectionStringDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = BogusGenerator.Internet.Ip();
            string sharedAccessKey = BogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act
            logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, startTime, duration);

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
        public void LogIotHubConnectionStringDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Lorem.Word();
            string deviceId = BogusGenerator.Lorem.Word();
            string sharedAccessKey = BogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName};DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal("Azure IoT Hub", dependency.DependencyType);
            Assert.Equal(iotHubName, dependency.DependencyName);
            Assert.Equal(iotHubName, dependency.TargetName);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal(value, Assert.Contains(key, dependency.Context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyConnectionString_WithoutConnectionString_Fails(string iotHubConnectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, startTime, duration));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogIotHubDependencyConnectionStringWithDependencyId_WithoutConnectionString_Fails(string iotHubConnectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.Random.Bool();

            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogIoTHubConnectionStringDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = BogusGenerator.Internet.Ip();
            string sharedAccessKey = BogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogIoTHubConnectionStringDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = BogusGenerator.Internet.Ip();
            string sharedAccessKey = BogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = BogusGenerator.Random.Bool();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependency(iotHubConnectionString: iotHubConnectionString, isSuccessful, startTime, duration, dependencyId));
        }

        [Fact]
        public void LogIotHubDependencyConnectionStringWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string iotHubName = BogusGenerator.Commerce.ProductName().Replace(" ", String.Empty);
            string deviceId = BogusGenerator.Internet.Ip();
            string sharedAccessKey = BogusGenerator.Random.Hash();
            var iotHubConnectionString = $"HostName={iotHubName}.;DeviceId={deviceId};SharedAccessKey={sharedAccessKey}";
            bool isSuccessful = BogusGenerator.Random.Bool();

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
