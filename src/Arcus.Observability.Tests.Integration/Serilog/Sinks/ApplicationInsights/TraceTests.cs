using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class TraceTests : ApplicationInsightsSinkTests
    {
        public TraceTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogInformationWithoutContext_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();

            // Act
            Logger.LogInformation("Trace message '{Sentence}'", message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Contains(message, result.Trace.Message);
                });
            });
        }

        [Fact]
        public async Task LogInformation_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string key = BogusGenerator.Lorem.Word();
            string expected = BogusGenerator.Lorem.Word();
            telemetryContext[key] = expected;

            // Act
            Logger.LogInformation("Trace message '{Sentence}'", telemetryContext, message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Contains(message, result.Trace.Message);
                    Assert.True(result.CustomDimensions.TryGetValue(key, out string actual), "Should contain custom dimension property");
                    Assert.Equal(expected, actual);
                });
            });
        }

        [Fact]
        public async Task LogTrace_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string key = BogusGenerator.Lorem.Word();
            string expected = BogusGenerator.Lorem.Word();
            telemetryContext[key] = expected;

            // Act
            Logger.LogTrace("Trace message '{Sentence}'", telemetryContext, message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Contains(message, result.Trace.Message);
                    Assert.True(result.CustomDimensions.TryGetValue(key, out string actual), "Should contain custom dimension property");
                    Assert.Equal(expected, actual);
                });
            });
        }

        [Fact]
        public async Task LogDebug_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string key = BogusGenerator.Lorem.Word();
            string expected = BogusGenerator.Lorem.Word();
            telemetryContext[key] = expected;

            // Act
            Logger.LogDebug("Trace message '{Sentence}'", telemetryContext, message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Contains(message, result.Trace.Message);
                    Assert.True(result.CustomDimensions.TryGetValue(key, out string actual), "Should contain custom dimension property");
                    Assert.Equal(expected, actual);
                });
            });
        }
    }
}