using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class HttpRequestDataTests : ApplicationInsightsSinkTests
    {
        public HttpRequestDataTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

         [Fact]
        public async Task LogRequest_SinksToApplicationInsightsWithCorrelation_ResultsInRequestTelemetry()
        {
            // Arrange
            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor);

            var operationName = "sampleoperation";
            HttpMethod httpMethod = GenerateHttpMethod();
            var requestUri = new Uri(BogusGenerator.Internet.UrlWithPath());
            HttpRequestData request = CreateStubRequest(httpMethod, requestUri.Scheme, requestUri.Host, requestUri.AbsolutePath);
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogRequest(request, statusCode, operationName, startTime, duration, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] results = await client.GetRequestsAsync();
                AssertX.Any(results, result =>
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

        private static HttpRequestData CreateStubRequest(HttpMethod method, string host, string path, string scheme)
        {
            var context = Mock.Of<FunctionContext>();

            var stub = new Mock<HttpRequestData>(context);
            stub.Setup(s => s.Method).Returns(method.Method);
            stub.Setup(s => s.Url).Returns(new Uri(scheme + "://" + host + path));

            return stub.Object;
        }
    }
}
