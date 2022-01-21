using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
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

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ServiceBusRequestTests>();

                // Act
                logger.LogServiceBusQueueRequest(serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotNull(results.Value);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(operationName, result.Request.Name);
                        Assert.Contains(queueName, result.Request.Source);
                        Assert.Contains(serviceBusNamespace, result.Request.Source);
                        Assert.Empty(result.Request.Url);
                        Assert.Equal(operationName, result.Operation.Name);
                        Assert.True(bool.TryParse(result.Request.Success, out bool success));
                        Assert.Equal(isSuccessful, success);

                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString());
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, queueName);
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace);
                    });
                });
            }
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

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ServiceBusRequestTests>();

                // Act
                logger.LogServiceBusTopicRequestWithSuffix(serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, duration, startTime, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotNull(results.Value);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(operationName, result.Request.Name);
                        Assert.Contains(topicName, result.Request.Source);
                        Assert.Contains(serviceBusNamespace, result.Request.Source);
                        Assert.Empty(result.Request.Url);
                        Assert.Equal(operationName, result.Operation.Name);
                        Assert.True(bool.TryParse(result.Request.Success, out bool success));
                        Assert.Equal(isSuccessful, success);

                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString());
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, topicName);
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace);
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName, subscriptionName);
                    });
                });
            }
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

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ServiceBusRequestTests>();

                // Act
                logger.LogServiceBusRequest(serviceBusNamespace, entityName, operationName, isSuccessful, duration, startTime, entityType, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotNull(results.Value);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(operationName, result.Request.Name);
                        Assert.Contains(entityName, result.Request.Source);
                        Assert.Contains(serviceBusNamespace, result.Request.Source);
                        Assert.Empty(result.Request.Url);
                        Assert.Equal(operationName, result.Operation.Name);
                        Assert.True(bool.TryParse(result.Request.Success, out bool success));
                        Assert.Equal(isSuccessful, success);

                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityType, entityType.ToString());
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.EntityName, entityName);
                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.RequestTracking.ServiceBus.Endpoint, serviceBusNamespace);
                    });
                });
            }
        }

        private static void AssertContainsCustomDimension(EventsResultDataCustomDimensions customDimensions, string key, string expected)
        {
            Assert.True(customDimensions.TryGetValue(key, out string actual), $"Cannot find {key} in custom dimensions: {String.Join(", ", customDimensions.Keys)}");
            Assert.Equal(expected, actual);
        }
    }
}
