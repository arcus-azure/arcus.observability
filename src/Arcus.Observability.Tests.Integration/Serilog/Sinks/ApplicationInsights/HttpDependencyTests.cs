﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
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

        [Fact]
        public async Task LogHttpDependencyWithRequest_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            HttpRequest request = CreateStubRequest(httpMethod, "arcus.test", "/integration", "https");
            string dependencyId = BogusGenerator.Random.Guid().ToString();
            
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
        private static HttpRequest CreateStubRequest(HttpMethod method, string host, string path, string scheme)
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