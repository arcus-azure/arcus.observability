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
    public class AzureServiceBusRequestLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogServiceBusTopicRequestWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subcriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subcriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutTopicName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutTopicName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement, context));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffixWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusTopicRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffix_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutEntityName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutEntityName_Fails(string topicName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequest_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusTopicRequestWithSuffix_WithoutSubscriptionName_Fails(string subscriptionName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context));
        }

        [Fact]
        public void LogServiceBusTopicRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffix_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffixWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement: null, context));
        }

        [Fact]
        public void LogServiceBusQueueRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffix_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, queueName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequest_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequest_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequest_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusQueueRequestWithSuffix_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, context));
        }

        [Fact]
        public void LogServiceBusQueueRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequest(serviceBusNamespace, entityName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffix_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSufix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSufix, entityName, operationName, isSuccessful, negativeDuration, startTime, context));
        }

        [Fact]
        public void LogServiceBusRequestWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusRequestWithSuffixWithMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, operationName, isSuccessful, measurement, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, topicName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement, entryType, context));
        }

        [Fact]
        public void LogServiceBusRequestWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, measurement: null, entityType, context));
        }

        [Fact]
        public void LogServiceBusRequestWithSuffixWithMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement: null, entityType, context));
        }

        [Fact]
        public void LogServiceBusRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Fact]
        public void LogServiceBusRequestWithSuffix_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequest_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutOperationName_Succeeds(string operationName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(ContextProperties.RequestTracking.ServiceBus.DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Contains(new KeyValuePair<string, object>(key, value), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityName, entityName), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString()), entry.Context);
            Assert.Contains(new KeyValuePair<string, object>(ContextProperties.General.TelemetryType, TelemetryType.Request), entry.Context);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequest_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutServiceBusNamespace_Fails(string serviceBusNamespace)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutServiceBusNamespaceSuffix_Fails(string serviceBusNamespaceSuffix)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequest_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogServiceBusRequestWithSuffix_WithoutEntityName_Fails(string entityName)
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entryType, context));
        }

        [Fact]
        public void LogServiceBusRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, negativeDuration, startTime, entryType, context));
        }

        [Fact]
        public void LogServiceBusRequestWithSuffix_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var entryType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            TimeSpan negativeDuration = _bogusGenerator.Date.Timespan().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogServiceBusRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, negativeDuration, startTime, entryType, context));
        }

        [Fact]
        public void LogServiceBusTopicRequest_WithContext_DoesNotAlterContext()
        {
            // Arrange
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogServiceBusTopicRequestWithSuffix_WithContext_DoesNotAlterContext()
        {
            // Arrange
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string suffix = _bogusGenerator.Lorem.Word();
            string topicName = _bogusGenerator.Lorem.Word();
            string subscriptionName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, suffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogServiceBusQueueRequest_WithContext_DoesNotAlterContext()
        {
            // Arrange
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogServiceBusQueueRequestWithSuffix_WithContext_DoesNotAlterContext()
        {
            // Arrange
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string suffix = _bogusGenerator.Lorem.Word();
            string queueName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogServiceBusQueueRequestWithSuffix(serviceBusNamespace, suffix, queueName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogServiceBusRequest_WithContext_DoesNotAlterContext()
        {
            // Arrange
            // Arrange
            var logger = new TestLogger();
            string serviceBusNamespace = _bogusGenerator.Lorem.Word();
            string entityName = _bogusGenerator.Lorem.Word();
            string operationName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var entityType = _bogusGenerator.Random.Enum<ServiceBusEntityType>();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
