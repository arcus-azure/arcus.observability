using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using static Arcus.Observability.Telemetry.Core.ContextProperties.RequestTracking;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class EventHubsRequestTests : ApplicationInsightsSinkTests
    {
        public EventHubsRequestTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogEventHubsRequest_SinksToApplicationInsights_ResultsInEventHubsRequestTelemetry()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            LoggerConfiguration.Enrich.WithComponentName(componentName);

            string eventHubsNamespace = BogusGenerator.Lorem.Word();
            string eventHubsConsumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();

            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogEventHubsRequest(eventHubsNamespace, eventHubsConsumerGroup, eventHubsName, operationName, isSuccessful, startTime, duration, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                RequestResult[] requests = await client.GetRequestsAsync();
                AssertX.Any(requests, result =>
                {
                    Assert.Equal(operationName, result.Name);
                    Assert.Contains(eventHubsName, result.Source);
                    Assert.Contains(eventHubsNamespace, result.Source);
                    Assert.True(string.IsNullOrWhiteSpace(result.Url), "request URL should be blank");
                    Assert.Equal(operationName, result.Operation.Name);
                    Assert.Equal(isSuccessful, result.Success);
                    Assert.Equal(componentName, result.RoleName);

                    AssertContainsCustomDimension(result.CustomDimensions, EventHubs.Namespace, eventHubsNamespace);
                    AssertContainsCustomDimension(result.CustomDimensions, EventHubs.ConsumerGroup, eventHubsConsumerGroup);
                    AssertContainsCustomDimension(result.CustomDimensions, EventHubs.Name, eventHubsName);
                });
            });
        }

        private static void AssertContainsCustomDimension(IDictionary<string, string> customDimensions, string key, string expected)
        {
            Assert.True(customDimensions.TryGetValue(key, out string actual), $"Cannot find {key} in custom dimensions: {String.Join(", ", customDimensions.Keys)}");
            Assert.Equal(expected, actual);
        }
    }
}
