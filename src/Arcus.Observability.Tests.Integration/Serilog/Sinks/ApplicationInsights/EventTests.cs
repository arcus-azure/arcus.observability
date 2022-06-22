using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
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

            AssertX.Any(GetLogEventsFromMemory(), logEvent => {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.EventTracking.EventLogEntry);
                Assert.NotNull(logEntry);

                var actualEventName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(EventLogEntry.EventName));
                Assert.Equal(eventName, actualEventName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(EventLogEntry.Context));
            });
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

            AssertX.Any(GetLogEventsFromMemory(), logEvent => {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.EventTracking.EventLogEntry);
                Assert.NotNull(logEntry);

                var actualEventName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(EventLogEntry.EventName));
                Assert.Equal(eventName, actualEventName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(EventLogEntry.Context));
            });
        }

        [Fact]
        public async Task LogEventWithCorrelationInfo_SinksToApplicationInsights_ResultsInTelemetryWithCorrelationInfo()
        {
            // Arrange
            string message = "Message that will be correlated";
            string operationId = $"operation-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            string operationParentId = $"operation-parent-{Guid.NewGuid()}";

            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(operationId, transactionId, operationParentId));

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithCorrelationInfo(correlationInfoAccessor)))
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

                    AssertX.Any(traceEvents.Value, trace =>
                    {
                        Assert.Equal(message, trace.Trace.Message);
                        Assert.True(trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.OperationId, out string actualOperationId), "Requires a operation ID in the custom dimensions");
                        Assert.True(trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.TransactionId, out string actualTransactionId), "Requires a transaction ID in the custom dimensions");
                        Assert.True(trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.OperationParentId, out string actualOperationParentId), "Requires a operation parent ID in the custom dimensions");

                        Assert.Equal(operationId, actualOperationId);
                        Assert.Equal(transactionId, actualTransactionId);
                        Assert.Equal(operationParentId, actualOperationParentId);
                        Assert.Equal(transactionId, trace.Operation.Id);
                        Assert.Equal(operationId, trace.Operation.ParentId);
                    });
                });
            }
        }

        [Fact]
        public async Task LogEventWithKubernetesInfo_SinksToApplicationInsights_ResultsInTelemetryWithKubernetesInfo()
        {
            // Arrange
            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";
            var message = "This message will have Kubernetes information";

            using (TemporaryEnvironmentVariable.Create(KubernetesEnricher.NodeNameVariable, nodeName))
            using (TemporaryEnvironmentVariable.Create(KubernetesEnricher.PodNameVariable, podName))
            using (TemporaryEnvironmentVariable.Create(KubernetesEnricher.NamespaceVariable, @namespace))
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
                               && trace.CustomDimensions.TryGetValue(ContextProperties.Kubernetes.NodeName, out string actualNodeName)
                               && nodeName == actualNodeName
                               && trace.CustomDimensions.TryGetValue(ContextProperties.Kubernetes.PodName, out string actualPodName)
                               && podName == actualPodName
                               && trace.CustomDimensions.TryGetValue(ContextProperties.Kubernetes.Namespace, out string actualNamespace)
                               && @namespace == actualNamespace;
                    });
                });
            }
        }
    }
}