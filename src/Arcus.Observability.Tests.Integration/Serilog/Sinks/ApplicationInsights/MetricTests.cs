using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class MetricTests : ApplicationInsightsSinkTests
    {
        public MetricTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogCustomMetric_SinksToApplicationInsights_ResultsInMetricTelemetry()
        {
            // Arrange
            string metricName = "threshold";
            double metricValue = 0.25;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogCustomMetric(metricName, metricValue, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsMetricsResult[] results = await client.GetMetricsAsync(metricName);
                AssertX.Any(results, metric =>
                {
                    Assert.Equal(metricName, metric.Name);
                    Assert.Equal(metricValue, metric.Value);

                    ContainsTelemetryContext(telemetryContext, metric);
                });
            });
        }

        private static void ContainsTelemetryContext(Dictionary<string, object> telemetryContext, EventsMetricsResult metric)
        {
            Assert.All(telemetryContext, item =>
            {
                string actual = Assert.Contains(item.Key, metric.CustomDimensions);
                Assert.Equal(item.Value.ToString(), actual);
            });
        }
    }
}