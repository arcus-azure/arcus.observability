using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class TraceTests : ApplicationInsightsSinkTests
    {
        public TraceTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogTrace_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogInformation("Trace message '{Sentence}' (Context: {Context})", message, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsTraceResult> results = await client.Events.GetTraceEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result => result.Trace.Message.Contains(message));
                });
            }
        }
    }
}