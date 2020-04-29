using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Serilog;
using Serilog.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    [Trait(name: "Category", value: "Integration")]
    public class ApplicationInsightsSinkTests : IntegrationTest
    {
        protected const string OnlyLastHourFilter = "timestamp gt now() sub duration'PT1H'";

        private readonly ITestOutputHelper _outputWriter;
        private readonly string _instrumentationKey;
        
        protected readonly Faker BogusGenerator = new Faker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public ApplicationInsightsSinkTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            _outputWriter = outputWriter;
            _instrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
        }

        protected ILoggerFactory CreateLoggerFactory(Action<LoggerConfiguration> configureLogging = null)
        {
            var configuration = new LoggerConfiguration()
                .WriteTo.Sink(new XunitLogEventSink(_outputWriter))
                .WriteTo.AzureApplicationInsights(_instrumentationKey);
            
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
            var client = new ApplicationInsightsDataClient(clientCredentials)
            {
                AppId = Configuration.GetValue<string>("ApplicationInsights:ApplicationId")
            };

            return client;
        }
    }
}
