using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Azure.ApplicationInsights.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class IoTHubTests : ApplicationInsightsSinkTests
    {
        public IoTHubTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogIoTHubDependency_SinksToApplicationInsights_ResultsIEventHubsDependencyTelemetry()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogIotHubDependency(iotHubName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "Azure IoT Hub"
                               && result.Dependency.Target == iotHubName
                               && result.Cloud.RoleName == componentName;
                    });
                });
            }
        }
    }
}
