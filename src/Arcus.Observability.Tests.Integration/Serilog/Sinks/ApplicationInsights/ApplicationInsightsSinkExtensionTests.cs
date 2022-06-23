using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class ApplicationInsightsSinkExtensionTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkExtensionTests" /> class.
        /// </summary>
        public ApplicationInsightsSinkExtensionTests(ITestOutputHelper outputWriter)
            : base(outputWriter)
        {
        }

        [Fact]
        public async Task Sink_WithConnectionString_WritesTelemetry()
        {
            // Arrange
            string connectionString = $"InstrumentationKey={InstrumentationKey}";
            var configuration = new LoggerConfiguration()
               .WriteTo.AzureApplicationInsightsWithConnectionString(connectionString);

            var message = "Something to log with connection string";
            ILogger logger = CreateLogger(configuration);

            // Act
            logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traces = await client.GetTracesAsync();
                AssertX.Any(traces, trace =>
                {
                    Assert.Equal(message, trace.Trace.Message);
                });
            });
        }

        [Fact]
        public async Task Sink_WithInstrumentationKey_WritesTelemetry()
        {
            // Arrange
            var configuration = new LoggerConfiguration()
                .WriteTo.AzureApplicationInsightsWithInstrumentationKey(InstrumentationKey);

            var message = "Something to log with connection string";
            ILogger logger = CreateLogger(configuration);

            // Act
            logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traces = await client.GetTracesAsync();
                AssertX.Any(traces, trace =>
                {
                    Assert.Equal(message, trace.Trace.Message);
                });
            });
        }
    }
}
