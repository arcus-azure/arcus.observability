using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
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
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogMetric(metricName, metricValue, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                var bodySchema = new MetricsPostBodySchema(
                    id: Guid.NewGuid().ToString(),
                    parameters: new MetricsPostBodySchemaParameters("customMetrics/" + metricName));

                MetricsResultsItem[] results = await client.GetMetricsAsync(bodySchema);
                Assert.NotEmpty(results);
            });
        }
    }
}