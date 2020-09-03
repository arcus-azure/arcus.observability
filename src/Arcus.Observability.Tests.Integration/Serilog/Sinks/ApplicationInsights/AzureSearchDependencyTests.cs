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
    public class AzureSearchDependencyTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public AzureSearchDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogAzureSearchDependency_SinksToApplicationInsights_ResultsInAzureSearchDependencyTelemetry()
        {
            // Arrange
            string searchServiceName = BogusGenerator.Commerce.Product();
            string operationName = BogusGenerator.Commerce.ProductName();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (IApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "Azure Search"
                               && result.Dependency.Target == searchServiceName
                               && result.Dependency.Data == operationName;
                    });
                });
            }
        }
    }
}
