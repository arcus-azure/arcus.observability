using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class EventTests : ApplicationInsightsSinkTests
    {
        public EventTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogEvent_SinksToApplicationInsights_ResultsInEventTelemetry()
        {
            // Arrange
            string eventName = BogusGenerator.Finance.AccountName();
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
                    EventsResults<EventsCustomEventResult> results = await client.Events.GetCustomEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(results.Value, result => result.CustomEvent.Name == eventName);
                });
            }
        }

        [Fact]
        public async Task LogEventWithComponentName_SinksToApplicationInsights_ResultsInTelemetryWithComponentName()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string componentName = BogusGenerator.Commerce.ProductName();
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
                    EventsResults<EventsTraceResult> results = await client.Events.GetTraceEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
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
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsCustomEventResult> events = await client.Events.GetCustomEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(events.Value, ev =>
                    {
                        return ev.CustomEvent.Name == eventName
                               && ev.CustomDimensions.TryGetValue(VersionEnricher.DefaultPropertyName, out string actualVersion)
                               && !String.IsNullOrWhiteSpace(actualVersion);
                    });
                });
            }
        }

        [Fact]
        public async Task LogEventWithCorrelationInfo_SinksToApplicationInsights_ResultsInTelemetryWithCorrelationInfo()
        {
            // Arrange
            var message = "Message 1/2 that will be correlated";
            
            string operationId = $"operation-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            
            DefaultCorrelationInfoAccessor.Instance.SetCorrelationInfo(new CorrelationInfo(operationId, transactionId));
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithCorrelationInfo()))
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
                    EventsResults<EventsTraceResult> traceEvents = await client.Events.GetTraceEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(traceEvents.Value, trace =>
                    {
                        return message == trace.Trace.Message
                               && trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.OperationId, out string actualOperationId)
                               && operationId == actualOperationId
                               && trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.TransactionId, out string actualTransactionId)
                               && transactionId == actualTransactionId;
                    });
                });
            }
        }

        [Fact]
        public async Task LogEventWithKubernetesInfo_SinksToApplicationInsights_ResultsInTelemetryWithKubernetesInfo()
        {
            // Arrange
            const string kubernetesNodeName = "KUBERNETES_NODE_NAME",
                         kubernetesPodName = "KUBERNETES_POD_NAME",
                         kubernetesNamespace = "KUBERNETES_NAMESPACE";

            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";
            var message = "This message will have Kubernetes information";

            using (TemporaryEnvironmentVariable.Create(kubernetesNodeName, nodeName))
            using (TemporaryEnvironmentVariable.Create(kubernetesPodName, podName))
            using (TemporaryEnvironmentVariable.Create(kubernetesNamespace, @namespace))
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithKubernetesInfo()))
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
                    EventsResults<EventsTraceResult> traceEvents = await client.Events.GetTraceEventsAsync(ApplicationId, filter: OnlyLastHourFilter);
                    Assert.Contains(traceEvents.Value, trace =>
                    {
                        return message == trace.Trace.Message
                               && trace.CustomDimensions.TryGetValue(kubernetesNodeName, out string actualNodeName)
                               && nodeName == actualNodeName
                               && trace.CustomDimensions.TryGetValue(kubernetesPodName, out string actualPodName)
                               && podName == actualPodName
                               && trace.CustomDimensions.TryGetValue(kubernetesNamespace, out string actualNamespace)
                               && @namespace == actualNamespace;
                    });
                });
            }
        }
    }
}