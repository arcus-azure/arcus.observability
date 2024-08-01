using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class ServiceBusRequestTests : ApplicationInsightsSinkTests
    {
        public ServiceBusRequestTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogServiceBusQueueRequestWithCorrelation_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor);

            string queueName = BogusGenerator.Lorem.Word();
            string serviceBusNamespace = $"{BogusGenerator.Lorem.Word()}.servicebus.windows.net";
            string operationName = BogusGenerator.Lorem.Sentence();
            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] results = await client.GetRequestsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(operationName, result.Name);
                    Assert.Contains(queueName, result.Source);
                    Assert.Contains(serviceBusNamespace, result.Source);
                    Assert.True(string.IsNullOrWhiteSpace(result.Url), "request URL should be blank");
                    Assert.Equal(operationName, result.Operation.Name);
                    Assert.Equal(isSuccessful, result.Success);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString());
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, queueName);
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace);

                    Assert.Equal(correlation.OperationId, result.Id);
                    Assert.Equal(correlation.TransactionId, result.Operation.Id);
                    Assert.Equal(correlation.OperationParentId, result.Operation.ParentId);
                });
            });
        }

        [Fact]
        public async Task LogServiceBusQueueRequest_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            string queueName = BogusGenerator.Lorem.Word();
            string serviceBusNamespace = $"{BogusGenerator.Lorem.Word()}.servicebus.windows.net";
            string operationName = BogusGenerator.Lorem.Sentence();
            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] results = await client.GetRequestsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(operationName, result.Name);
                    Assert.Contains(queueName, result.Source);
                    Assert.Contains(serviceBusNamespace, result.Source);
                    Assert.True(string.IsNullOrWhiteSpace(result.Url), "request URL should be blank");
                    Assert.Equal(operationName, result.Operation.Name);
                    Assert.Equal(isSuccessful, result.Success);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString());
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, queueName);
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace);
                });
            });
        }

        [Fact]
        public async Task LogServiceBusTopicRequestWithSuffix_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            string topicName = BogusGenerator.Lorem.Word();
            string subscriptionName = BogusGenerator.Lorem.Word();
            string serviceBusNamespace = BogusGenerator.Lorem.Word();
            string serviceBusNamespaceSuffix = ".servicebus.windows.de";
            string operationName = BogusGenerator.Lorem.Sentence();
            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] results = await client.GetRequestsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(operationName, result.Name);
                    Assert.Contains(topicName, result.Source);
                    Assert.Contains(serviceBusNamespace, result.Source);
                    Assert.Contains(serviceBusNamespaceSuffix, result.Source);
                    Assert.True(string.IsNullOrWhiteSpace(result.Url), "request URL should be blank");
                    Assert.Equal(operationName, result.Operation.Name);
                    Assert.Equal(isSuccessful, result.Success);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString());
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, topicName);
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace + serviceBusNamespaceSuffix);
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName);
                });
            });
        }

        [Fact]
        public async Task LogServiceBusRequest_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            string entityName = BogusGenerator.Lorem.Word();
            string serviceBusNamespace = $"{BogusGenerator.Lorem.Word()}.servicebus.windows.net";
            string operationName = BogusGenerator.Lorem.Sentence();
            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            var entityType = BogusGenerator.PickRandom<ServiceBusEntityType>();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            TestLocation = TestLocation.Remote;

            // Act
            Logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] results = await client.GetRequestsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(operationName, result.Name);
                    Assert.Contains(entityName, result.Source);
                    Assert.Contains(serviceBusNamespace, result.Source);
                    Assert.True(string.IsNullOrWhiteSpace(result.Url), "request URL should be blank");
                    Assert.Equal(operationName, result.Operation.Name);
                    Assert.Equal(isSuccessful, result.Success);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString());
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, entityName);
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace);
                });
            });
        }

        private static void AssertContainsCustomDimension(IDictionary<string, string> customDimensions, string key, string expected)
        {
            Assert.Equal(expected, Assert.Contains(key, customDimensions));
        }
    }
}
