using System;
using System.Linq;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Integration.Fixture;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
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
            string expectedProperty = BogusGenerator.Lorem.Word();
            var exception = new TestException(message) { SpyProperty = expectedProperty };
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
                    EventsResults<EventsExceptionResult> results = await client.Events.GetExceptionEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Exception.OuterMessage == exception.Message
                               && result.CustomDimensions.Keys.Contains($"Exception-{nameof(TestException.SpyProperty)}") == false;
                    });
                });
            }
        }

        [Fact]
        public async Task LogException_SinksToApplicationInsightsWithIncludedProperties_ResultsInExceptionTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string expectedProperty = BogusGenerator.Lorem.Word();
            var exception = new TestException(message) { SpyProperty = expectedProperty };
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureOptions: options => options.Exception.IncludeProperties = true))
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
                    EventsResults<EventsExceptionResult> results = await client.Events.GetExceptionEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Exception.OuterMessage == exception.Message
                               && result.CustomDimensions.TryGetValue($"Exception-{nameof(TestException.SpyProperty)}", out string actualProperty)
                               && expectedProperty == actualProperty;
                    });
                });
            }
        }

        [Fact]
        public async Task LogExceptionWithCustomPropertyFormat_SinksToApplicationInsights_ResultsInExceptionTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string expectedProperty = BogusGenerator.Lorem.Word();
            var exception = new TestException(message) { SpyProperty = expectedProperty };
            string propertyFormat = "Exception.{0}";
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureOptions: options =>
            {
                options.Exception.IncludeProperties = true;
                options.Exception.PropertyFormat = propertyFormat;
            }))
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
                    EventsResults<EventsExceptionResult> results = await client.Events.GetExceptionEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result =>
                    {
                        string propertyName = String.Format(propertyFormat, nameof(TestException.SpyProperty));
                        return result.Exception.OuterMessage == exception.Message
                               && result.CustomDimensions.TryGetValue(propertyName, out string actualProperty)
                               && expectedProperty == actualProperty;
                    });
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
                    EventsResults<EventsExceptionResult> results = await client.Events.GetExceptionEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Exception.OuterMessage == exception.Message && result.Cloud.RoleName == componentName);
                });
            }
        }
    }
}