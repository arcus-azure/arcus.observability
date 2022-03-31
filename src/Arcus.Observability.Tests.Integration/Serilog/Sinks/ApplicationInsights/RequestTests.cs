using System;
using System.Collections.Generic;
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
        public async Task LogRequest_SinksToApplicationInsightsWithoutOperationNameAndCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpResponse response = CreateStubResponse(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == $"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}"
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && Guid.TryParse(result.Request.Id, out Guid _)
                               && result.Operation.Name.StartsWith(httpMethod.Method);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithOperationNameWithoutCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            string operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpResponse response = CreateStubResponse(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == $"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}"
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && Guid.TryParse(result.Request.Id, out Guid _)
                               && result.Operation.Name == $"{httpMethod} {operationName}";
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithResponseStatusCodeWithoutOperationNameWithCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var requestId = Guid.NewGuid().ToString();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureOptions: options => options.Request.GenerateId = () => requestId))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                HttpResponse response = CreateStubResponse(statusCode);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == $"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}"
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && result.Request.Id == requestId
                               && result.Operation.Name.StartsWith(httpMethod.Method);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithResponseStatusCodeWithOperationNameAndCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            string operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequest request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var requestId = Guid.NewGuid().ToString();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureOptions: options => options.Request.GenerateId = () => requestId))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, (int) statusCode, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == $"{requestUri.Scheme}://{requestUri.Host}{requestUri.AbsolutePath}"
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && result.Request.Id == requestId
                               && result.Operation.Name == $"{httpMethod.Method} {operationName}";
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseWithoutOperationNameWithCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var requestId = Guid.NewGuid().ToString();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureOptions: options => options.Request.GenerateId = () => requestId))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUri);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, statusCode, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == requestUri.ToString()
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && result.Request.Id == requestId
                               && result.Operation.Name.StartsWith(httpMethod.Method);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseWithOperationNameWithoutCustomId_ResultsInRequestTelemetry()
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
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == requestUri.ToString()
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && Guid.TryParse(result.Request.Id, out Guid _)
                               && result.Operation.Name == $"{httpMethod.Method} {operationName}";
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }
        
        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseStatusCodeWithoutOperationNameAndCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUri);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, statusCode, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == requestUri.ToString()
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && Guid.TryParse(result.Request.Id, out Guid _)
                               && result.Operation.Name.StartsWith(httpMethod.Method);
                    });
                });
            }

            VerifyLogEventProperties(requestUri);
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsightsWithResponseStatusCodeWithOperationNameAndCustomId_ResultsInRequestTelemetry()
        {
            // Arrange
            string operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var requestId = Guid.NewGuid().ToString();
            
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureOptions: options => options.Request.GenerateId = () => requestId))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUri);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, statusCode, operationName, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Request.Url == requestUri.ToString()
                               && result.Request.ResultCode == ((int) statusCode).ToString()
                               && result.Request.Id == requestId
                               && result.Operation.Name == $"{httpMethod.Method} {operationName}";
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