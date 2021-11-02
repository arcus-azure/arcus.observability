using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        public async Task LogServiceBusTopicRequest_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            string serviceBusNamespace = $"{BogusGenerator.Lorem.Word()}.servicebus.windows.net";
            string topicName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                // Act
                logger.LogServiceBusTopicRequest(serviceBusNamespace, topicName, operationName, isSuccessful, duration, startTime, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.);
                })
            }
        }
    }
}
