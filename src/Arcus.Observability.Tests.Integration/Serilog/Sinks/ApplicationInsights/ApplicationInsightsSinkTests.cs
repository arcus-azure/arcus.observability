using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Tests.Core;
using Bogus;
using Microsoft.Azure.ApplicationInsights.Query;
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
        private readonly string _instrumentationKey;

        protected readonly Faker BogusGenerator = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _outputWriter = outputWriter;
            _memoryLogSink = new InMemoryLogSink();
            _instrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");

            ApplicationId = Configuration.GetValue<string>("ApplicationInsights:ApplicationId");
        }

        /// <summary>
        /// Gets the ID of the application that has access to the Azure Application Insights resource.
        /// </summary>
        protected string ApplicationId { get; }

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
                .WriteTo.AzureApplicationInsights(_instrumentationKey, configureOptions)
                .WriteTo.Sink(_memoryLogSink);

            configureLogging?.Invoke(configuration);
            return LoggerFactory.Create(builder => builder.AddSerilog(configuration.CreateLogger(), dispose: true));
        }

        protected Dictionary<string, object> CreateTestTelemetryContext([CallerMemberName] string memberName = "")
        {
            var operationId = Guid.NewGuid();
            Logger.LogInformation("Testing '{TestName}' using {OperationId}", memberName, operationId);

            return new Dictionary<string, object>
            {
                ["OperationId"] = operationId,
                ["TestName"] = memberName
            };
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

        protected IEnumerable<LogEvent> GetLogEventsFromMemory()
        {
            return _memoryLogSink.CurrentLogEmits;
        }
    }
}
