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
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogEvent(eventName, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsCustomEventResult[] results = await client.GetEventsAsync();
                Assert.Contains(results, result => result.CustomEvent.Name == eventName);
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
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                Assert.Contains(results, result => result.Trace.Message == message && result.Cloud.RoleName == componentName);
            });
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
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client=>
            {
                EventsCustomEventResult[] events = await client.GetEventsAsync();
                Assert.Contains(events, ev =>
                {
                    return ev.CustomEvent.Name == eventName
                           && ev.CustomDimensions.TryGetValue(VersionEnricher.DefaultPropertyName, out string actualVersion)
                           && !String.IsNullOrWhiteSpace(actualVersion);
                });
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
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traceEvents = await client.GetTracesAsync();
                AssertX.Any(traceEvents, trace =>
                {
                    Assert.Equal(message, trace.Trace.Message);
                    Assert.True(trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.OperationId, out string actualOperationId), "Requires a operation ID in the custom dimensions");
                    Assert.True(trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.TransactionId, out string actualTransactionId), "Requires a transaction ID in the custom dimensions");
                    Assert.True(trace.CustomDimensions.TryGetValue(ContextProperties.Correlation.OperationParentId, out string actualOperationParentId), "Requires a operation parent ID in the custom dimensions");

                    Assert.Equal(operationId, actualOperationId);
                    Assert.Equal(transactionId, actualTransactionId);
                    Assert.Equal(operationParentId, actualOperationParentId);
                    Assert.Equal(operationId, trace.Operation.Id);
                    Assert.Equal(operationParentId, trace.Operation.ParentId);
                });
            });
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
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traceEvents = await client.GetTracesAsync();
                Assert.Contains(traceEvents, trace =>
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