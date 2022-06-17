using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Tests.Core;
using Bogus;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    [Trait(name: "Category", value: "Integration")]
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        protected const string OnlyLastHourFilter = "timestamp gt now() sub duration'PT1H'";
        protected const string PastHalfHourTimeSpan = "PT30M";

        private readonly ITestOutputHelper _outputWriter;
        private readonly InMemoryLogSink _memoryLogSink;

        protected readonly Faker BogusGenerator = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _outputWriter = outputWriter;
            _memoryLogSink = new InMemoryLogSink();
            
            InstrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            ApplicationId = Configuration.GetValue<string>("ApplicationInsights:ApplicationId");
        }

        /// <summary>
        /// Gets the instrumentation key to connect to the Azure Application Insights instance.
        /// </summary>
        protected string InstrumentationKey { get; }

        /// <summary>
        /// Gets the ID of the application that has access to the Azure Application Insights resource.
        /// </summary>
        protected string ApplicationId { get; }

        protected CorrelationInfo GenerateCorrelationInfo()
        {
            return new CorrelationInfo(
                $"operation-{Guid.NewGuid()}",
                $"transaction-{Guid.NewGuid()}",
                $"parent-{Guid.NewGuid()}");
        }

        /// <summary>
        /// Creates an <see cref="ILoggerFactory"/> instance that will create <see cref="Microsoft.Extensions.Logging.ILogger"/> instances that writes to Azure Application Insights.
        /// </summary>
        /// <param name="configureLogging">The optional function to configure additional logging features.</param>
        /// <param name="configureOptions">The optional function to configure additional properties while writing to Azure Application Insights.</param>
        protected ILoggerFactory CreateLoggerFactory(
            Action<LoggerConfiguration> configureLogging = null,
            Action<ApplicationInsightsSinkOptions> configureOptions = null)
        {
            var configuration = new LoggerConfiguration()
                .WriteTo.Sink(new XunitLogEventSink(_outputWriter))
                .WriteTo.AzureApplicationInsightsWithInstrumentationKey(InstrumentationKey, configureOptions)
                .WriteTo.Sink(_memoryLogSink);

            configureLogging?.Invoke(configuration);
            return CreateLoggerFactory(configuration);
        }

        /// <summary>
        /// Creates an <see cref="ILoggerFactory"/> instance that will create <see cref="Microsoft.Extensions.Logging.ILogger"/> instances that writes to Azure Application Insights.
        /// </summary>
        /// <param name="configuration">The Serilog configuration to setup the Microsoft logging.</param>
        protected ILoggerFactory CreateLoggerFactory(
            LoggerConfiguration configuration)
        {
            return LoggerFactory.Create(builder => builder.AddSerilog(configuration.CreateLogger(), dispose: true));
        }

        protected Dictionary<string, object> CreateTestTelemetryContext([CallerMemberName] string memberName = "")
        {
            var testId = Guid.NewGuid();
            Logger.LogInformation("Testing '{TestName}' using {TestId}", memberName, testId);

            return new Dictionary<string, object>
            {
                ["TestId"] = testId,
                ["TestName"] = memberName
            };
        }

        protected async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<ApplicationInsightsClient, Task> assertion)
        {
            using (IApplicationInsightsDataClient dataClient = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    var client = new ApplicationInsightsClient(dataClient, ApplicationId);
                    await assertion(client);
                });
            }
        }

        protected async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<Task> assertion)
        {
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(assertion, timeout: TimeSpan.FromMinutes(7));
        }

        protected async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<Task> assertion, TimeSpan timeout)
        {
            await Policy.TimeoutAsync(timeout)
                        .WrapAsync(Policy.Handle<Exception>()
                                         .WaitAndRetryForeverAsync(index => TimeSpan.FromSeconds(1)))
                        .ExecuteAsync(assertion);
        }
        protected ApplicationInsightsDataClient CreateApplicationInsightsClient()
        {
            var clientCredentials = new ApiKeyClientCredentials(Configuration.GetValue<string>("ApplicationInsights:ApiKey"));
            var client = new ApplicationInsightsDataClient(clientCredentials);

            return client;
        }

        protected void AssertRequestCorrelation(EventsRequestResult requestResult)
        {
           
        }

        protected IEnumerable<LogEvent> GetLogEventsFromMemory()
        {
            return _memoryLogSink.CurrentLogEmits;
        }
    }

    public class ApplicationInsightsClient
    {
        private readonly IApplicationInsightsDataClient _client;
        private readonly string _applicationId;
        private const string PastHalfHourTimeSpan = "PT30M";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsClient" /> class.
        /// </summary>
        public ApplicationInsightsClient(
            IApplicationInsightsDataClient client,
            string applicationId)
        {
            _client = client;
            _applicationId = applicationId;
        }

        public async Task<IEnumerable<EventsDependencyResult>> GetDependenciesAsync()
        {
            EventsResults<EventsDependencyResult> result = 
                await _client.Events.GetDependencyEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            return result.Value;
        }

        public async Task<IEnumerable<EventsRequestResult>> GetRequestsAsync()
        {
            EventsResults<EventsRequestResult> result = 
                await _client.Events.GetRequestEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            return result.Value;
        }

        public async Task<IEnumerable<EventsCustomEventResult>> GetCustomEventsAsync()
        {
            EventsResults<EventsCustomEventResult> result = 
                await _client.Events.GetCustomEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            return result.Value;
        }
    }
}
