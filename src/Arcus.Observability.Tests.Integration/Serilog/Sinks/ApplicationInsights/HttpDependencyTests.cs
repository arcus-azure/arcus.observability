using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Moq;
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
        public async Task LogHttpDependencyWithRequestMessageWithCorrelation_SinksToApplicationInsights_ResultsInHttpDependencyTelemetryWithComponentName()
        {
            // Arrange
            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);

            string componentName = BogusGenerator.Commerce.ProductName();
            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = BogusGenerator.Image.LoremFlickrUrl();
            string dependencyId = BogusGenerator.Random.Word();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(
                       config => config.Enrich.WithCorrelationInfo(accessor)
                                       .Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUrl)
                {
                    Content = new StringContent(BogusGenerator.Lorem.Paragraph())
                };
                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            var requestUri = new Uri(requestUrl);
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(DependencyType, result.Dependency.Type);
                        Assert.Equal(requestUri.Host, result.Dependency.Target);
                        Assert.Equal($"{httpMethod} {requestUri.AbsolutePath}", result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                        Assert.Equal(componentName, result.Cloud.RoleName);

                        Assert.Equal(correlation.OperationId, result.Operation.ParentId);
                        Assert.Equal(correlation.TransactionId, result.Operation.Id);
                    });
                });
            }
            
            AssertSerilogLogProperties(httpMethod, requestUri.Host, requestUri.AbsolutePath);
        }

        [Fact]
        public async Task LogHttpDependencyWithRequestMessage_SinksToApplicationInsights_ResultsInHttpDependencyTelemetryWithComponentName()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = BogusGenerator.Image.LoremFlickrUrl();
            string dependencyId = BogusGenerator.Random.Word();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUrl)
                {
                    Content = new StringContent(BogusGenerator.Lorem.Paragraph())
                };
                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            var requestUri = new Uri(requestUrl);
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(DependencyType, result.Dependency.Type);
                        Assert.Equal(requestUri.Host, result.Dependency.Target);
                        Assert.Equal($"{httpMethod} {requestUri.AbsolutePath}", result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                        Assert.Equal(componentName, result.Cloud.RoleName);
                    });
                });
            }
            
            AssertSerilogLogProperties(httpMethod, requestUri.Host, requestUri.AbsolutePath);
        }

        [Fact]
        public async Task LogHttpDependencyWithRequest_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            HttpRequest request = CreateStubRequest(httpMethod, "arcus.test", "/integration", "https");
            string dependencyId = BogusGenerator.Random.Guid().ToString();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                DateTimeOffset startTime = DateTimeOffset.Now;
                var duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(DependencyType, result.Dependency.Type);
                        Assert.Equal(request.Host.Host, result.Dependency.Target);
                        Assert.Equal($"{httpMethod} {request.Path}", result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                    });
                });
            }

            AssertSerilogLogProperties(httpMethod, request.Host.Host, request.Path.Value);
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
        private static HttpRequest CreateStubRequest(HttpMethod method, string host, string path, string scheme)
        {
            var stubRequest = new Mock<HttpRequest>();
            stubRequest.Setup(request => request.Method).Returns(method.ToString());
            stubRequest.Setup(request => request.Host).Returns(new HostString(host));
            stubRequest.Setup(request => request.Path).Returns(path);
            stubRequest.Setup(req => req.Scheme).Returns(scheme);

            return stubRequest.Object;
        }

        private void AssertSerilogLogProperties(HttpMethod httpMethod, string host, string path)
        {
            IEnumerable<LogEvent> logEvents = GetLogEventsFromMemory();
            AssertX.Any(logEvents, logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);

                LogEventProperty actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(DependencyType, actualDependencyType.Value.ToDecentString(), true);

                LogEventProperty actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal($"{httpMethod} {path}", actualDependencyName.Value.ToDecentString());

                LogEventProperty actualTargetName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(host, actualTargetName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}