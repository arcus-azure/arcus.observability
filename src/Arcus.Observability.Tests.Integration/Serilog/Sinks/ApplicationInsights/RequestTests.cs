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
    public class RequestTests : ApplicationInsightsSinkTests
    {
        public RequestTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

         [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithCorrelation_ResultsInRequestTelemetry()
        {
            // Arrange
            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);

            var operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureLogging: config => config.Enrich.WithCorrelationInfo(accessor)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpResponse response = CreateStubResponse(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = DateTimeOffset.Now;
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal($"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}", result.Request.Url);
                        Assert.Equal(((int) statusCode).ToString(), result.Request.ResultCode);
                        Assert.Equal($"{httpMethod.Method} {operationName}", result.Operation.Name);

                        Assert.Equal(correlation.OperationId, result.Request.Id);
                        Assert.Equal(correlation.TransactionId, result.Operation.Id);
                        Assert.Equal(correlation.OperationParentId, result.Operation.ParentId);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithoutCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            var operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpResponse response = CreateStubResponse(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = DateTimeOffset.Now;
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal($"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}", result.Request.Url);
                        Assert.Equal(((int) statusCode).ToString(), result.Request.ResultCode);
                        Assert.Equal($"{httpMethod.Method} {operationName}", result.Operation.Name);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithResponseWithCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            var operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var requestId = Guid.NewGuid().ToString();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(
                       configureLogging: config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, null),
                       configureOptions: options => options.Request.GenerateId = () => requestId))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                HttpResponse response = CreateStubResponse(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = DateTimeOffset.Now;
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal($"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}", result.Request.Url);
                        Assert.Equal(((int)statusCode).ToString(), result.Request.ResultCode);
                        Assert.True(Guid.TryParse(result.Request.Id, out Guid _));
                        Assert.Equal($"{httpMethod.Method} {operationName}", result.Operation.Name);
                        Assert.Equal(requestId, result.Request.Id);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseWithCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            var operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var requestId = Guid.NewGuid().ToString();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(
                       configureLogging: config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, null),
                       configureOptions: options => options.Request.GenerateId = () => requestId))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUri);
                var response = new HttpResponseMessage(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = DateTimeOffset.Now;
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(requestUri.ToString(), result.Request.Url);
                        Assert.Equal(((int)statusCode).ToString(), result.Request.ResultCode);
                        Assert.Equal(requestId, result.Request.Id);
                        Assert.Equal($"{httpMethod.Method} {operationName}", result.Operation.Name);
                        Assert.Equal(requestId, result.Request.Id);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseWithoutCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            string operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUri);
                var response = new HttpResponseMessage(statusCode);

                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = DateTimeOffset.Now;
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(requestUri.ToString(), result.Request.Url);
                        Assert.Equal(((int)statusCode).ToString(), result.Request.ResultCode);
                        Assert.Equal($"{httpMethod.Method} {operationName}", result.Operation.Name);
                    });
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

        private void VerifyLogEventProperties(Uri requestUri)
        {
            IEnumerable<LogEvent> logEvents = GetLogEventsFromMemory();
            AssertX.Any(logEvents, logEvent => 
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.RequestTracking.RequestLogEntry);
                Assert.NotNull(logEntry);

                LogEventProperty actualRequestHost = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(RequestLogEntry.RequestHost));
                Assert.Equal($"{requestUri.Scheme}://{requestUri.Host}", actualRequestHost.Value.ToDecentString());

                LogEventProperty actualRequestUri = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(RequestLogEntry.RequestUri));
                Assert.Equal(requestUri.AbsolutePath, actualRequestUri.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(RequestLogEntry.Context));
            });
        }
    }
}