using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Arcus.Testing;
using Bogus;
using GuardNet;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using InMemoryLogSink = Arcus.Observability.Tests.Core.InMemoryLogSink;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public enum TestLocation { Local, Remote }

    [Trait(name: "Category", value: "Integration")]
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly InMemoryLogSink _memoryLogSink;
        private readonly InMemoryApplicationInsightsTelemetryConverter _telemetrySink;


        /// <summary>
        /// Gets the test generator to create bogus content during the integration test.
        /// </summary>
        protected static readonly Faker BogusGenerator = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        protected ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _memoryLogSink = new InMemoryLogSink();

            _testOutput = outputWriter;
            ApplicationInsightsSinkOptions = new ApplicationInsightsSinkOptions();
            LoggerConfiguration = new LoggerConfiguration();
            InstrumentationKey = Configuration.GetValue<string>("Arcus:ApplicationInsights:InstrumentationKey");
            _telemetrySink = new InMemoryApplicationInsightsTelemetryConverter();
        }

        /// <summary>
        /// Gets or sets the test location to determine where the telemetry should be logged to.
        /// </summary>
        protected TestLocation TestLocation { get; set; }

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
                ApplicationInsightsSinkOptions userOptions = ApplicationInsightsSinkOptions;
                _telemetrySink.Options = userOptions;

                LoggerConfiguration
                    .MinimumLevel.Verbose()
                    .WriteTo.Sink(new XunitLogEventSink(_testOutput))
                    .WriteTo.Sink(_memoryLogSink);

                switch (TestLocation)
                {
                    case TestLocation.Local:
                        LoggerConfiguration.WriteTo.ApplicationInsights(_telemetrySink);
                        break;

                    case TestLocation.Remote:
                        LoggerConfiguration.WriteTo.AzureApplicationInsightsWithConnectionString("InstrumentationKey=" + InstrumentationKey, options =>
                        {
                            options.Correlation.OperationIdPropertyName = userOptions.Correlation.OperationIdPropertyName;
                            options.Correlation.OperationParentIdPropertyName = userOptions.Correlation.OperationParentIdPropertyName;
                            options.Correlation.TransactionIdPropertyName = userOptions.Correlation.TransactionIdPropertyName;

                            options.Exception.IncludeProperties = userOptions.Exception.IncludeProperties;
                            options.Exception.PropertyFormat = userOptions.Exception.PropertyFormat;

                            options.Request.GenerateId = userOptions.Request.GenerateId;
                        });
                        break;
                }

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

            _telemetrySink.Options = ApplicationInsightsSinkOptions;
            config.WriteTo.ApplicationInsights(_telemetrySink);

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
            _testOutput.WriteLine("Testing '{0}' using {1}", memberName, testId);

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
        /// <exception cref="TimeoutException">Thrown when the <paramref name="assertion"/> failed to be verified within the configured timeout.</exception>
        protected async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<ITelemetryQueryClient, Task> assertion)
        {
            Guard.NotNull(assertion, nameof(assertion));

            if (TestLocation is TestLocation.Remote)
            {
                var client = new AppInsightsClient(Configuration);
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(
                    async () => await assertion(client),
                    timeout: TimeSpan.FromMinutes(5));
            }
            else if (TestLocation is TestLocation.Local)
            {
                var client = new InMemoryTelemetryQueryClient(_telemetrySink);
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(
                    async () => await assertion(client),
                    TimeSpan.FromSeconds(5));
            }
        }

        private async Task RetryAssertUntilTelemetryShouldBeAvailableAsync(Func<Task> assertion, TimeSpan timeout)
        {
            await Poll.UntilAvailableAsync<XunitException>(assertion, options =>
            {
                options.Interval = TimeSpan.FromSeconds(1);
                options.Timeout = timeout;
                options.FailureMessage = $"({TestLocation}) Telemetry should be available in Application Insights but it wasn't within the given timeout";
            });
        }
    }
}
