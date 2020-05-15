using System;
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
    public class ExceptionTests : ApplicationInsightsSinkTests
    {
        public ExceptionTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogException_SinksToApplicationInsights_ResultsInExceptionTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            var exception = new PlatformNotSupportedException(message);
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogCritical(exception, exception.Message);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsExceptionResult> results = await client.GetExceptionEventsAsync(filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result => result.Exception.OuterMessage == exception.Message);
                });
            }
        }

        [Fact]
        public async Task LogExceptionWithComponentName_SinksToApplicationInsights_ResultsInTelemetryWithComponentName()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string componentName = BogusGenerator.Commerce.ProductName();
            var exception = new PlatformNotSupportedException(message);
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogCritical(exception, exception.Message);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsExceptionResult> results = await client.GetExceptionEventsAsync(filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Exception.OuterMessage == exception.Message && result.Cloud.RoleName == componentName);
                });
            }
        }
    }
}