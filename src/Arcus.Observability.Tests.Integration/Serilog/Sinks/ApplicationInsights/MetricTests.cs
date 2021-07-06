using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog.Events;
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

                    IList<MetricsResultsItem> results = await client.Metrics.GetMultipleAsync(ApplicationId, new List<MetricsPostBodySchema> { bodySchema });
                    Assert.NotEmpty(results);
                });
            }

            Assert.Contains(GetLogEventsFromMemory(), logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.MetricTracking.MetricLogEntry);
                return logEntry != null
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(MetricLogEntry.MetricName))?.Value.ToDecentString() == metricName
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(MetricLogEntry.MetricValue))?.Value.ToDecentString() == metricValue.ToString("0.00", CultureInfo.InvariantCulture)
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(MetricLogEntry.Context)) != null;
            });
        }
    }
}