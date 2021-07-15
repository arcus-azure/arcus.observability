using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class HttpDependencyTests : ApplicationInsightsSinkTests
    {
        private const string DependencyType = "HTTP";

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
                        return result.Dependency.Type == DependencyType
                               && result.Dependency.Target == requestUri.Host
                               && result.Dependency.Name == $"{httpMethod} {requestUri.AbsolutePath}";
                    });
                });
            }

            AssertX.Any(GetLogEventsFromMemory(), logEvent => {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);

                var actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(DependencyType, actualDependencyType.Value.ToDecentString(), true);

                var actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal($"{httpMethod} {requestUri.AbsolutePath}", actualDependencyName.Value.ToDecentString());

                var actualTargetName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(requestUri.Host, actualTargetName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
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
                        return result.Dependency.Type == DependencyType
                               && result.Dependency.Target == requestUri.Host
                               && result.Dependency.Name == $"{httpMethod} {requestUri.AbsolutePath}"
                               && result.Cloud.RoleName == componentName;
                    });
                });
            }

            AssertX.Any(GetLogEventsFromMemory(), logEvent => {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);

                var actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(DependencyType, actualDependencyType.Value.ToDecentString(), true);

                var actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal($"{httpMethod} {requestUri.AbsolutePath}", actualDependencyName.Value.ToDecentString());

                var actualTargetName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(requestUri.Host, actualTargetName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
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