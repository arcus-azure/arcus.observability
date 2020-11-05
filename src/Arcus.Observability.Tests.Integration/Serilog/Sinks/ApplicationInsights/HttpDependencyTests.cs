using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class HttpDependencyTests : ApplicationInsightsSinkTests
    {
        public HttpDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogHttpDependency_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = BogusGenerator.Image.LoremFlickrUrl();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                var request = new HttpRequestMessage(httpMethod, requestUrl)
                {
                    Content = new StringContent(BogusGenerator.Lorem.Paragraph())
                };
                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset(days: 0);
                var duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, telemetryContext);
            }

            // Assert
            var requestUri = new Uri(requestUrl);
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "HTTP"
                               && result.Dependency.Target == requestUri.Host
                               && result.Dependency.Name == $"{httpMethod} {requestUri.AbsolutePath}";
                    });
                });
            }
        }

        [Fact]
        public async Task LogHttpDependencyWithComponentName_SinksToApplicationInsights_ResultsInHttpDependencyTelemetryWithComponentName()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = BogusGenerator.Image.LoremFlickrUrl();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUrl)
                {
                    Content = new StringContent(BogusGenerator.Lorem.Paragraph())
                };
                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                var duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogHttpDependency(request, statusCode, DateTimeOffset.UtcNow, duration, telemetryContext);
            }

            // Assert
            var requestUri = new Uri(requestUrl);
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "HTTP"
                               && result.Dependency.Target == requestUri.Host
                               && result.Dependency.Name == $"{httpMethod} {requestUri.AbsolutePath}"
                               && result.Cloud.RoleName == componentName;
                    });
                });
            }
        }

        private HttpMethod GenerateHttpMethod()
        {
            return BogusGenerator.PickRandom(
                HttpMethod.Get,
                HttpMethod.Delete,
                HttpMethod.Head,
                HttpMethod.Options,
                HttpMethod.Patch,
                HttpMethod.Post,
                HttpMethod.Put,
                HttpMethod.Trace);
        }
    }
}