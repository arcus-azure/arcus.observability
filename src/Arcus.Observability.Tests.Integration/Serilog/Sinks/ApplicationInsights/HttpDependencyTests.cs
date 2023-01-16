using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Xunit;
using Xunit.Abstractions;

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
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor)
                               .Enrich.WithComponentName(componentName);

            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = BogusGenerator.Image.LoremFlickrUrl();
            string dependencyId = BogusGenerator.Random.Word();

            HttpRequestMessage request = CreateHttpRequestMessage(httpMethod, requestUrl);
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId, telemetryContext);

            // Assert
            var requestUri = new Uri(requestUrl);
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
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

        [Fact]
        public async Task LogHttpDependencyWithRequestMessage_SinksToApplicationInsights_ResultsInHttpDependencyTelemetryWithComponentName()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            LoggerConfiguration.Enrich.WithComponentName(componentName);

            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = BogusGenerator.Image.LoremFlickrUrl();
            string dependencyId = BogusGenerator.Random.Word();

            HttpRequestMessage request = CreateHttpRequestMessage(httpMethod, requestUrl);
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId, telemetryContext);

            // Assert
            var requestUri = new Uri(requestUrl);
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(DependencyType, result.Dependency.Type);
                    Assert.Equal(requestUri.Host, result.Dependency.Target);
                    Assert.Equal($"{httpMethod} {requestUri.AbsolutePath}", result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                    Assert.Equal(componentName, result.Cloud.RoleName);
                });
            });
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, string requestUrl)
        {
            var request = new HttpRequestMessage(httpMethod, requestUrl)
            {
                Content = new StringContent(BogusGenerator.Lorem.Paragraph())
            };
            return request;
        }

        [Fact]
        public async Task LogHttpDependencyWithRequest_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            HttpRequest request = CreateHttpRequest(httpMethod, "arcus.test", "/integration", "https");
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = DateTimeOffset.Now;
            var duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(DependencyType, result.Dependency.Type);
                    Assert.Equal(request.Host.Host, result.Dependency.Target);
                    Assert.Equal($"{httpMethod} {request.Path}", result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);
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
        private static HttpRequest CreateHttpRequest(HttpMethod method, string host, string path, string scheme)
        {
            var stubRequest = new Mock<HttpRequest>();
            stubRequest.Setup(request => request.Method).Returns(method.ToString());
            stubRequest.Setup(request => request.Host).Returns(new HostString(host));
            stubRequest.Setup(request => request.Path).Returns(path);
            stubRequest.Setup(req => req.Scheme).Returns(scheme);

            return stubRequest.Object;
        }
    }
}