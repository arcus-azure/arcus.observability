using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    [Trait(name: "Category", value: "Integration")]
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        private readonly string _instrumentationKey;
        private readonly Faker _bogusGenerator = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _instrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
        }

        [Fact]
        public void LogTrace_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                string message = _bogusGenerator.Lorem.Sentence();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogInformation("Trace message '{Sentence}' (Context: {Context})", message, telemetryContext);

                // Assert
                // Hold on till we have agreed on assertion... 
            }
        }

        [Fact]
        public void LogEvent_SinksToApplicationInsights_ResultsInEventTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
                string eventName = _bogusGenerator.Finance.AccountName();

                // Act
                logger.LogEvent(eventName, telemetryContext);

                // Assert
                // Hold on till we have agreed on assertion... 
            }
        }

        [Fact]
        public void LogMetric_SinksToApplicationInsights_ResultsInMetricTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
                string metricName = _bogusGenerator.Vehicle.Fuel();
                double metricValue = _bogusGenerator.Random.Double();

                // Act
                logger.LogMetric(metricName, metricValue, telemetryContext);

                // Assert
                // Hold on till we have agreed on assertion... 
            }
        }

        [Fact]
        public void LogRequest_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpMethod httpMethod = GenerateHttpMethod();
                string host = _bogusGenerator.Company.CompanyName().Replace(" ", "");
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
        }

        [Fact]
        public void LogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                string dependencyType = _bogusGenerator.Lorem.Word();
                object dependencyData = _bogusGenerator.Finance.Account();
                bool isSuccessful = _bogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, telemetryContext);

                // Assert
                // Hold on till we have agreed on assertion...
            }
        }

        [Fact]
        public void LogHttpDependency_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpMethod httpMethod = GenerateHttpMethod();
                string requestUri = _bogusGenerator.Image.LoremFlickrUrl();
                var request = new HttpRequestMessage(httpMethod, requestUri)
                {
                    Content = new StringContent(_bogusGenerator.Lorem.Paragraph())
                };
                var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
                var duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, telemetryContext);

                // Assert
                // Hold on till we have agreed on assertion... 
            }
        }

        [Fact]
        public void LogSqlDependency_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                string serverName = _bogusGenerator.Database.Engine();
                string databaseName = _bogusGenerator.Database.Collation();
                string tableName = _bogusGenerator.Database.Column();
                string operation = _bogusGenerator.PickRandom("GET", "UPDATE", "DELETE", "CREATE");
                bool isSuccessful = _bogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogSqlDependency(serverName, databaseName, tableName, operation, isSuccessful, startTime, duration, telemetryContext);

                // Assert
                // Hold on till we have agreed on assertion...
            }
        }

        [Fact]
        public void LogEventWithComponentName_SinksToApplicationInsights_ResultsInTelemetryWithComponentName()
        {
            // Arrange
            string componentName = _bogusGenerator.Commerce.ProductName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogInformation("This message will be enriched with a component name");

                // Assert
                // Hold on till we have agreed on assertion...
            }
        }

        [Fact]
        public void LogEventWithVersion_SinksToApplicationInsights_ResultsInTelemetryWithVersion()
        {
            // Arrange
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithVersion()))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogEvent("Update version");

                // Assert
                // Hold on till we have agreed on assertion...
            }
        }

        [Fact]
        public void LogEventWithCorrelationInfo_SinksToApplicationInsights_ResultsInTelemetryWithCorrelationInfo()
        {
            // Arrange
            string operationId = $"operation-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            DefaultCorrelationInfoAccessor.Instance.SetCorrelationInfo(new CorrelationInfo(operationId, transactionId));
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithCorrelationInfo()))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogInformation("Message 1/2 that will be correlated");
                logger.LogInformation("Message 2/2 that will be correlated");

                // Assert
                // Hold on till we have agreed on assertion...
            }
        }

        [Fact]
        public void LogEventWithKubernetesInfo_SinksToApplicationInsights_ResultsInTelemetryWithKubernetesInfo()
        {
            // Arrange
            const string kubernetesNodeName = "KUBERNETES_NODE_NAME",
                         kubernetesPodName = "KUBERNETES_POD_NAME",
                         kubernetesNamespace = "KUBERNETES_NAMESPACE";

            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";

            using (TemporaryEnvironmentVariable.Create(kubernetesNodeName, nodeName))
            using (TemporaryEnvironmentVariable.Create(kubernetesPodName, podName))
            using (TemporaryEnvironmentVariable.Create(kubernetesNamespace, @namespace))
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithKubernetesInfo()))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogInformation("This message will have Kubernetes information");
                
                // Assert
                // Hold on till we have agreed on assertion...
            }
        }

        [Fact]
        public void LogHttpDependencyWithComponentName_SinksToApplicationInsights_ResultsInHttpDependencyTelemetryWithComponentName()
        {
            // Arrange
            string componentName = _bogusGenerator.Commerce.ProductName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

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
        }

        private ILoggerFactory CreateLoggerFactory(Action<LoggerConfiguration> configureLogging = null)
        {
            var configuration = new LoggerConfiguration().WriteTo.AzureApplicationInsights(_instrumentationKey);
            configureLogging?.Invoke(configuration);
            return LoggerFactory.Create(builder => builder.AddSerilog(configuration.CreateLogger(), dispose: true));
        }

        private Dictionary<string, object> CreateTestTelemetryContext([CallerMemberName] string memberName = "")
        {
            var operationId = Guid.NewGuid();
            Logger.LogInformation("Testing '{TestName}' using {OperationId}", memberName, operationId);

            return new Dictionary<string, object>
            {
                ["OperationId"] = operationId,
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
