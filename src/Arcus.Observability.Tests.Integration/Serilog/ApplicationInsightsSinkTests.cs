using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        private readonly string _instrumentationKey;
        private readonly Faker _bogusGenerator = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _instrumentationKey = Configuration.GetValue<string>("Arcus:ApplicationInsights:InstrumentationKey");
        }

        [Fact]
        public void LogTrace_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            ILogger logger = CreateLogger();
            string message = _bogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            logger.LogInformation("Trace message '{Sentence}' (Context: {Context})", message, telemetryContext);

            // Assert
            // Hold on till we have agreed on assertion...
        }

        [Fact]
        public void LogEvent_SinksToApplicationInsights_ResultsInEventTelemetry()
        {
            // Arrange
            ILogger logger = CreateLogger();

            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string eventName = _bogusGenerator.Finance.AccountName();

            // Act
            logger.LogEvent(eventName, telemetryContext);

            // Assert
            // Hold on till we have agreed on assertion...
        }

        [Fact]
        public void LogMetric_SinksToApplicationInsights_ResultsInMetricTelemetry()
        {
            // Arrange
            ILogger logger = CreateLogger();
            
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string metricName = _bogusGenerator.Vehicle.Fuel();
            double metricValue = _bogusGenerator.Random.Double();

            // Act
            logger.LogMetric(metricName, metricValue, telemetryContext);

            // Assert
            // Hold on till we have agreed on assertion...
        }

        [Fact]
        public void LogRequest_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            ILogger logger = CreateLogger();

            HttpMethod httpMethod = GenerateHttpMethod();
            string host = _bogusGenerator.Company.CompanyName();
            string path = "/" + _bogusGenerator.Commerce.ProductName();
            HttpRequest request = CreateStubRequest(httpMethod, "https", host, path);

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            HttpResponse response = CreateStubResponse(statusCode);
            
            var duration = _bogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            
            // Act
            logger.LogRequest(request, response, duration, telemetryContext);

            // Assert
            // Hold on till we have agreed on assertion...
        }

        [Fact]
        public void LogHttpDependency_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            ILogger logger = CreateLogger();

            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUri = _bogusGenerator.Image.LoremFlickrUrl();
            var request = new HttpRequestMessage(httpMethod, requestUri)
            {
                Content = new StringContent(_bogusGenerator.Lorem.Paragraph())
            };
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var duration = _bogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            logger.LogHttpDependency(request, statusCode, DateTimeOffset.UtcNow, duration, telemetryContext);

            // Assert
            // Hold on till we have agreed on assertion...
        }

        [Fact]
        public void LogSqlDependency_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            ILogger logger = CreateLogger();
            string serverName = _bogusGenerator.Database.Engine();
            string databaseName = _bogusGenerator.Database.Collation();
            string tableName = _bogusGenerator.Database.Column();
            string operation = _bogusGenerator.PickRandom("GET", "UPDATE", "DELETE", "CREATE");
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            logger.LogSqlDependency(serverName, databaseName, tableName, operation, isSuccessful, DateTimeOffset.UtcNow, duration, telemetryContext);

            // Assert
            // Hold on till we have agreed on assertion...
        }

        private ILogger CreateLogger()
        {
            Logger logger = new LoggerConfiguration()
                .WriteTo.AzureApplicationInsights(_instrumentationKey)
                .CreateLogger();

            var factory = new SerilogLoggerFactory(logger);
            return factory.CreateLogger<ApplicationInsightsSinkTests>();
        }

        private static Dictionary<string, object> CreateTestTelemetryContext([CallerMemberName] string memberName = "")
        {
            return new Dictionary<string, object>
            {
                ["OperationId"] = Guid.NewGuid(),
                ["TestName"] = memberName
            };
        }

        private HttpMethod GenerateHttpMethod()
        {
            return _bogusGenerator.PickRandom(
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
    }
}
