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
    public class MetricTests : ApplicationInsightsSinkTests
    {
        public MetricTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogMetric_SinksToApplicationInsights_ResultsInMetricTelemetry()
        {
            // Arrange
            string metricName = "threshold";
            double metricValue = 0.25;
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogMetric(metricName, metricValue, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    var bodySchema = new MetricsPostBodySchema(
                        id: Guid.NewGuid().ToString(), 
                        parameters: new MetricsPostBodySchemaParameters("customMetrics/" + metricName));

                    IList<MetricsResultsItem> results = await client.GetMetricsAsync(new List<MetricsPostBodySchema> { bodySchema });
                    Assert.NotEmpty(results);
                });
            }
        }
    }
}