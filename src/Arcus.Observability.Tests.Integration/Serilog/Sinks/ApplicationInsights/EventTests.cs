using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class EventTests : ApplicationInsightsSinkTests
    {
        public EventTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogCustomEvent_SinksToApplicationInsights_ResultsInEventTelemetry()
        {
            // Arrange
            string eventName = BogusGenerator.Finance.AccountName();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogCustomEvent(eventName, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsCustomEventResult[] results = await client.GetCustomEventsAsync();
                AssertX.Any(results, ev =>
                {
                    Assert.Equal(eventName, ev.Name);
                });
            });
        }

        [Fact]
        public async Task LogEventWithComponentName_SinksToApplicationInsights_ResultsInTelemetryWithComponentName()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string componentName = BogusGenerator.Commerce.ProductName();
            LoggerConfiguration.Enrich.WithComponentName(componentName);
            
            // Act
            Logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] results = await client.GetTracesAsync();
                AssertX.Any(results, trace =>
                {
                    Assert.Equal(message, trace.Trace.Message);
                    Assert.Equal(componentName, trace.RoleName);
                });
            });
        }

        [Fact]
        public async Task LogCustomEventWithVersion_SinksToApplicationInsights_ResultsInTelemetryWithVersion()
        {
            // Arrange
            var eventName = "Update version";
            LoggerConfiguration.Enrich.WithVersion();
            TestLocation = TestLocation.Remote;

            // Act
            Logger.LogCustomEvent(eventName);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsCustomEventResult[] events = await client.GetCustomEventsAsync();
                AssertX.Any(events, ev =>
                {
                    Assert.Equal(eventName, ev.Name);
                    string actualVersion = Assert.Contains(VersionEnricher.DefaultPropertyName, ev.CustomDimensions);
                    Assert.False(string.IsNullOrWhiteSpace(actualVersion), "enriched event version should not be blank");
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
            LoggerConfiguration.Enrich.WithCorrelationInfo(correlationInfoAccessor);
            
            // Act
            Logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traceEvents = await client.GetTracesAsync();
                AssertX.Any(traceEvents, trace =>
                {
                    Assert.Equal(message, trace.Trace.Message);
                    Assert.Equal(operationId, Assert.Contains(ContextProperties.Correlation.OperationId, trace.CustomDimensions));
                    Assert.Equal(transactionId, Assert.Contains(ContextProperties.Correlation.TransactionId, trace.CustomDimensions));
                    Assert.Equal(operationParentId, Assert.Contains(ContextProperties.Correlation.OperationParentId, trace.CustomDimensions));

                    Assert.Equal(transactionId, trace.Operation.Id);
                    Assert.Equal(operationId, trace.Operation.ParentId);
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
            {
                LoggerConfiguration.Enrich.WithKubernetesInfo();

                // Act
                Logger.LogInformation(message);
            }

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traceEvents = await client.GetTracesAsync();
                AssertX.Any(traceEvents, trace =>
                {
                    Assert.Equal(message, trace.Trace.Message);
                    Assert.Equal(nodeName, Assert.Contains(ContextProperties.Kubernetes.NodeName, trace.CustomDimensions));
                    Assert.Equal(podName, Assert.Contains(ContextProperties.Kubernetes.PodName, trace.CustomDimensions));
                    Assert.Equal(@namespace, Assert.Contains(ContextProperties.Kubernetes.Namespace, trace.CustomDimensions));
                });
            });
        }
    }
}