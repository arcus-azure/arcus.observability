using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Azure.ApplicationInsights.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Moq;
using Polly;
using Polly.Timeout;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    [Trait(name: "Category", value: "Integration")]
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        private const string TestNameKey = "TestName";
        private const string OnlyLastHourFilter = "timestamp gt now() sub duration'PT1H'";

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
        public async Task LogTrace_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            string message = _bogusGenerator.Lorem.Sentence();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogInformation("Trace message '{Sentence}' (Context: {Context})", message, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsTraceResult> results = await client.GetTraceEventsAsync(filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result => result.Trace.Message.Contains(message));
                });
            }
        }

        [Fact]
        public async Task LogEvent_SinksToApplicationInsights_ResultsInEventTelemetry()
        {
            // Arrange
            string eventName = _bogusGenerator.Finance.AccountName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogEvent(eventName, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsCustomEventResult> results = await client.GetCustomEventsAsync(filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result => result.CustomEvent.Name == eventName);
                });
            }
        }

        [Fact]
        public async Task LogMetric_SinksToApplicationInsights_ResultsInMetricTelemetry()
        {
            // Arrange
            string metricName = _bogusGenerator.Vehicle.Fuel();
            double metricValue = 0.25;
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogMetric(metricName, metricValue, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    var bodySchema = new MetricsPostBodySchema(
                        id: Guid.NewGuid().ToString(), 
                        parameters: new MetricsPostBodySchemaParameters("customMetrics/" + metricName));

                    IList<MetricsResultsItem> results = await client.GetMetricsAsync(new List<MetricsPostBodySchema> { bodySchema });
                    Assert.NotEmpty(results);
                });
            }
        }

        [Fact]
        public async Task LogRequest_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            string host = _bogusGenerator.Company.CompanyName().Replace(" ", "");
            string path = "/" + _bogusGenerator.Commerce.ProductName();
            string scheme = "https";
            HttpRequest request = CreateStubRequest(httpMethod, scheme, host, path);

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
                HttpResponse response = CreateStubResponse(statusCode);

                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.GetRequestEventsAsync(filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.Url == $"{scheme}://{host.ToLower()}{path}");
                });
            }
        }

        [Fact]
        public async Task LogRequestMessage_SinksToApplicationInsights_ResultsInRequestTelemetry()
        {
            // Arrange
            var requestUri = new Uri(_bogusGenerator.Internet.Url());
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                HttpMethod httpMethod = GenerateHttpMethod();
                var request = new HttpRequestMessage(httpMethod, requestUri);

                var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
                var response = new HttpResponseMessage(statusCode);

                var duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogRequest(request, response, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsRequestResult> results = await client.GetRequestEventsAsync(filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Request.Url == requestUri.ToString());
                });
            }
        }

        [Fact]
        public async Task LogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Arcus";
            string dependencyData = _bogusGenerator.Finance.Account();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                bool isSuccessful = _bogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Dependency.Type == dependencyType && result.Dependency.Data == dependencyData);
                });
            }
        }

        [Fact]
        public async Task LogServiceBusDependency_SinksToApplicationInsights_ResultsInServiceBusDependencyTelemetry()
        {
            // Arrange
            string entityName = _bogusGenerator.Commerce.Product();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = _bogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, ServiceBusEntityType.Queue, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Dependency.Type == "Azure Service Bus" && result.Dependency.Target == entityName);
                });
            }
        }

        [Fact]
        public async Task LogTableStorageDependency_SinksToApplicationInsights_ResultsInTableStorageDependencyTelemetry()
        {
            // Arrange
            string componentName = _bogusGenerator.Commerce.ProductName();
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = _bogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogTableStorageDependency(tableName, accountName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "Azure Table Storage"
                               && result.Dependency.Target == accountName
                               && result.Dependency.Data == tableName
                               && result.Cloud.RoleName == componentName;
                    });
                });
            }
        }

        [Fact]
        public async Task LogHttpDependency_SinksToApplicationInsights_ResultsInHttpDependencyTelemetry()
        {
            // Arrange
            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = _bogusGenerator.Image.LoremFlickrUrl();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                
                var request = new HttpRequestMessage(httpMethod, requestUrl)
                {
                    Content = new StringContent(_bogusGenerator.Lorem.Paragraph())
                };
                var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset(days: 0);
                var duration = _bogusGenerator.Date.Timespan();
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
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "HTTP"
                               && result.Dependency.Target == requestUri.Host
                               && result.Dependency.Name == $"{httpMethod} {requestUri.AbsolutePath}";
                    });
                });
            }
        }

        [Fact]
        public async Task LogSqlDependency_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            string serverName = _bogusGenerator.Database.Engine();
            string databaseName = _bogusGenerator.Database.Collation();
            string tableName = _bogusGenerator.Database.Column();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                string operation = _bogusGenerator.PickRandom("GET", "UPDATE", "DELETE", "CREATE");
                bool isSuccessful = _bogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = _bogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogSqlDependency(serverName, databaseName, tableName, operation, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "SQL"
                               && result.Dependency.Target == serverName
                               && result.Dependency.Name == $"SQL: {databaseName}/{tableName}";
                    });
                });
            }
        }

        [Fact]
        public async Task LogEventWithComponentName_SinksToApplicationInsights_ResultsInTelemetryWithComponentName()
        {
            // Arrange
            string message = _bogusGenerator.Lorem.Sentence();
            string componentName = _bogusGenerator.Commerce.ProductName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogInformation(message);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsTraceResult> results = await client.GetTraceEventsAsync(filter: OnlyLastHourFilter);
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Trace.Message == message && result.Cloud.RoleName == componentName);
                });
            }
        }

        [Fact]
        public async Task LogEventWithVersion_SinksToApplicationInsights_ResultsInTelemetryWithVersion()
        {
            // Arrange
            var eventName = "Update version";
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithVersion()))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                // Act
                logger.LogEvent(eventName);
            }

            // Assert
            // Hold on till we have agreed on assertion...
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
        public async Task LogHttpDependencyWithComponentName_SinksToApplicationInsights_ResultsInHttpDependencyTelemetryWithComponentName()
        {
            // Arrange
            string componentName = _bogusGenerator.Commerce.ProductName();
            HttpMethod httpMethod = GenerateHttpMethod();
            string requestUrl = _bogusGenerator.Image.LoremFlickrUrl();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                var request = new HttpRequestMessage(httpMethod, requestUrl)
                {
                    Content = new StringContent(_bogusGenerator.Lorem.Paragraph())
                };
                var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
                var duration = _bogusGenerator.Date.Timespan();
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
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "HTTP"
                               && result.Dependency.Target == requestUri.Host
                               && result.Dependency.Name == $"{httpMethod} {requestUri.AbsolutePath}"
                               && result.Cloud.RoleName == componentName;
                    });
                });
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

        private static async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<Task> assertion)
        {
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(assertion, timeout: TimeSpan.FromMinutes(7));
        }

        private static async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<Task> assertion, TimeSpan timeout)
        {
            await Policy.TimeoutAsync(timeout)
                        .WrapAsync(Policy.Handle<XunitException>()
                                         .WaitAndRetryForeverAsync(index => TimeSpan.FromSeconds(1)))
                        .ExecuteAsync(assertion);
        }

        private ApplicationInsightsDataClient CreateApplicationInsightsClient()
        {
            var clientCredentials = new ApiKeyClientCredentials(Configuration.GetValue<string>("ApplicationInsights:ApiKey"));
            var client = new ApplicationInsightsDataClient(clientCredentials)
            {
                AppId = Configuration.GetValue<string>("ApplicationInsights:ApplicationId")
            };

            return client;
        }
    }
}
