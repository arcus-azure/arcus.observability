using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class RequestTests : ApplicationInsightsSinkTests
    {
        public RequestTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithResponse_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.Url());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                HttpResponse response = CreateStubResponse(statusCode);

                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.Url == $"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}");
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithResponseStatusCode_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.Url());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, (int)statusCode, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.Url == $"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}");
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponse_ResultsInRequestTelemetry()
        {
            // Arrange
            var requestUri = new Uri(BogusGenerator.Internet.Url());
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpMethod httpMethod = GenerateHttpMethod();
                var request = new HttpRequestMessage(httpMethod, requestUri);

                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                var response = new HttpResponseMessage(statusCode);

                var duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.Url == requestUri.ToString());
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseStatusCode_ResultsInRequestTelemetry()
        {
            // Arrange
            var requestUri = new Uri(BogusGenerator.Internet.Url());
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpMethod httpMethod = GenerateHttpMethod();
                var request = new HttpRequestMessage(httpMethod, requestUri);

                var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
                var duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, statusCode, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.Url == requestUri.ToString());
                });
            }

            VerifyLogEventProperties(requestUri);
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

        private static HttpRequest CreateStubRequest(HttpMethod httpMethod, string requestScheme, string host, string path)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(req => req.Method).Returns(httpMethod.ToString().ToUpper);
            request.Setup(req => req.Scheme).Returns(requestScheme);
            request.Setup(req => req.Host).Returns(new HostString(host));
            request.Setup(req => req.Path).Returns(path);

            return request.Object;
        }

        private static HttpResponse CreateStubResponse(HttpStatusCode statusCode)
        {
            var response = new Mock<HttpResponse>();
            response.Setup(res => res.StatusCode).Returns((int) statusCode);

            return response.Object;
        }

        private void VerifyLogEventProperties(Uri requestUri){
            AssertX.Any(GetLogEventsFromMemory(), logEvent => {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.RequestTracking.RequestLogEntry);
                Assert.NotNull(logEntry);

                var actualRequestHost = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(RequestLogEntry.RequestHost));
                Assert.Equal($"{requestUri.Scheme}://{requestUri.Host}", actualRequestHost.Value.ToDecentString());

                var actualRequestUri = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(RequestLogEntry.RequestUri));
                Assert.Equal(requestUri.AbsolutePath, actualRequestUri.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(RequestLogEntry.Context));
            });
        }
    }
}