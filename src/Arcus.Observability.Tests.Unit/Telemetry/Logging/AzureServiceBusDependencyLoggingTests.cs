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
    public class AzureServiceBusDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

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
        public void LogServiceBusDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, dependencyId, entityType);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(entityName, dependency.TargetName);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(entityType.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            const ServiceBusEntityType entityType = ServiceBusEntityType.Queue;
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, dependencyId, entityType));
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
        public void LogServiceBusDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, measurement, entityType);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(entityName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(entityName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(entityType.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyIdWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string entityName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusDependency(entityName, isSuccessful, measurement, dependencyId, entityType);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(entityName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(entityName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(entityType.ToString(), entityTypeItem.Value);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusDependencyWithDurationMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement, entityType));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusDependencyWithDependencyIdWithDurationMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement, dependencyId, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Commerce.Product();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement: (DurationMeasurement)null, entityType));
        }

        [Fact]
        public void LogServiceBusDependencyWithDependencyIdWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Commerce.Product();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusDependency(entityName, isSuccessful, measurement: (DurationMeasurement)null, dependencyId, entityType));
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
        public void LogServiceBusQueueDependencyWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            var startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(queueName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(queueName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Queue.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusQueueDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyId_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            var startTime = DateTimeOffset.UtcNow;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration, dependencyId));
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
        public void LogServiceBusQueueDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(queueName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(queueName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Queue.ToString(), entityTypeItem.Value);
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyIdWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(queueName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(queueName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Queue.ToString(), entityTypeItem.Value);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueDependencyWithDurationMeasurement_WithoutQueueName_Fails(string queueName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueDependencyWithDependencyIdWithDurationMeasurement_WithoutQueueName_Fails(string queueName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement, dependencyId));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithDependencyIdWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            string dependencyId = _bogusGenerator.Lorem.Word();

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogServiceBusQueueDependency(queueName, isSuccessful, measurement: (DurationMeasurement)null, dependencyId));
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
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
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
        public void LogServiceBusTopicDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Equal(topicName, dependency.TargetName);
            Assert.Equal("Azure Service Bus", dependency.DependencyType);
            Assert.Equal(topicName, dependency.DependencyName);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(isSuccessful, dependency.IsSuccessful);
            KeyValuePair<string, object> entityTypeItem = Assert.Single(dependency.Context, item => item.Key == ContextProperties.DependencyTracking.ServiceBus.EntityType);
            Assert.Equal(ServiceBusEntityType.Topic.ToString(), entityTypeItem.Value);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicDependencyWithDurationMeasurement_WithoutTopicName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement));
        }

        [Fact]
        public void LogServiceBusTopicDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicDependency(topicName, isSuccessful, measurement: (DurationMeasurement)null));
        }
    }
}
