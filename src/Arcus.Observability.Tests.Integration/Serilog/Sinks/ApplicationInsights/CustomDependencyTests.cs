using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Azure.ApplicationInsights.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class CustomDependencyTests : ApplicationInsightsSinkTests
    {
        public CustomDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Arcus";
            string dependencyData = BogusGenerator.Finance.Account();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Dependency.Type == dependencyType && result.Dependency.Data == dependencyData);
                });
            }
        }
    }
}