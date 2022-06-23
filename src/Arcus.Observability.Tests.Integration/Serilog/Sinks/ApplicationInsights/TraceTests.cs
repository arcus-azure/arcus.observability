using System.Collections.Generic;
using System.Threading.Tasks;
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
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogInformation("Trace message '{Sentence}' (Context: {Context})", message, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                Assert.Contains(results, result => result.Trace.Message.Contains(message));
            });
        }
    }
}