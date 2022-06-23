using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Arcus.Observability.Tests.Core;
using Bogus;
using GuardNet;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    [Trait(name: "Category", value: "Integration")]
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        private readonly ITestOutputHelper _outputWriter;
        private readonly InMemoryLogSink _memoryLogSink;

        /// <summary>
        /// Gets the test generator to create bogus content during the integration test.
        /// </summary>
        protected static readonly Faker BogusGenerator = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _outputWriter = outputWriter;
            _memoryLogSink = new InMemoryLogSink();

            ApplicationInsightsSinkOptions = new ApplicationInsightsSinkOptions();
            LoggerConfiguration = new LoggerConfiguration();
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

        /// <summary>
        /// Gets the configuration options to influence the behavior of the Application Insights Serilog sink.
        /// </summary>
        protected ApplicationInsightsSinkOptions ApplicationInsightsSinkOptions { get; }

        /// <summary>
        /// Gets the default integration test Serilog logger configuration which already includes the Application Insights Serilog sink.
        /// </summary>
        protected LoggerConfiguration LoggerConfiguration { get; }

        /// <summary>
        /// Gets the logger implementation that writes telemetry via Serilog to Application Insights.
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                LoggerConfiguration
                    .WriteTo.Sink(new XunitLogEventSink(_outputWriter))
                    .WriteTo.ApplicationInsights(InstrumentationKey, ApplicationInsightsTelemetryConverter.Create(ApplicationInsightsSinkOptions))
                    .WriteTo.Sink(_memoryLogSink);

                ILogger logger = CreateLogger(LoggerConfiguration);
                return logger;
            }
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> implementation that uses the Serilog <paramref name="config"/> behind the scenes.
        /// </summary>
        /// <remarks>
        ///     For most common cases, the <see cref="Logger"/> property should be sufficient.
        ///     Use this for use cases where a custom <see cref="LoggerConfiguration"/> is required.
        /// </remarks>
        /// <param name="config">The Serilog configuration to setup the logger.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="config"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the <paramref name="config"/> already has created the logger.</exception>
        protected ILogger CreateLogger(LoggerConfiguration config)
        {
            Guard.NotNull(config, nameof(config), "Requires a Serilog logger configuration instance to setup the test logger used during the test");

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(config.CreateLogger(), dispose: true));
            ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

            return logger;
        }

        /// <summary>
        /// Creates a default telemetry context that provides test information, which can be passed along during the telemetry logging.
        /// </summary>
        /// <param name="memberName">The generated test method member name which will be added to the telemetry context (no action required).</param>
        protected Dictionary<string, object> CreateTestTelemetryContext([CallerMemberName] string memberName = "")
        {
            var testId = Guid.NewGuid();
            _outputWriter.WriteLine("Testing '{0}' using {1}", memberName, testId);

            return new Dictionary<string, object>
            {
                ["TestId"] = testId,
                ["TestName"] = memberName
            };
        }

        /// <summary>
        /// Asserts on whether the logged telemetry availability on Application Insights by retrying the provided <paramref name="assertion"/>.
        /// </summary>
        /// <param name="assertion">The assertion function that takes in an Application Insights client which provides access to the available telemetry.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="assertion"/> is <c>null</c>.</exception>
        /// <exception cref="TimeoutRejectedException">Thrown when the <paramref name="assertion"/> failed to be verified within the configured timeout.</exception>
        protected async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<ApplicationInsightsClient, Task> assertion)
        {
            Guard.NotNull(assertion, nameof(assertion), "Requires an assertion function to correctly verify if the logged telemetry is tracked in Application Insights");

            using (ApplicationInsightsDataClient dataClient = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    var client = new ApplicationInsightsClient(dataClient, ApplicationId);
                    await assertion(client);
                }, timeout: TimeSpan.FromMinutes(7)); 
            }
        }

        private ApplicationInsightsDataClient CreateApplicationInsightsClient()
        {
            var clientCredentials = new ApiKeyClientCredentials(Configuration.GetValue<string>("ApplicationInsights:ApiKey"));
            var client = new ApplicationInsightsDataClient(clientCredentials);

            return client;
        }

        private static async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<Task> assertion, TimeSpan timeout)
        {
            PolicyResult result =
                await Policy.TimeoutAsync(timeout)
                            .WrapAsync(Policy.Handle<Exception>()
                                             .WaitAndRetryForeverAsync(index => TimeSpan.FromSeconds(1)))
                            .ExecuteAndCaptureAsync(assertion);

            if (result.Outcome is OutcomeType.Failure)
            {
                if (result.FinalException is TimeoutRejectedException
                    && result.FinalException.InnerException != null)
                {
                    throw result.FinalException.InnerException;
                }

                throw result.FinalException;
            }
        }
    }
}
