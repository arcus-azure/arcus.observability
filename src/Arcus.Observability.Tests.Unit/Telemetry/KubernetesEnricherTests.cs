using System;
using Arcus.Observability.Telemetry.Serilog;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    [Trait("Category", "Unit")]
    public class KubernetesEnricherTests
    {
        [Fact]
        public void LogEvent_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            const string kubernetesNodeName = "KUBERNETES_NODE_NAME",
                         kubernetesPodName = "KUBERNETES_POD_NAME",
                         kubernetesNamespace = "KUBERNETES_NAMESPACE";

            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            var logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.Create(kubernetesNodeName, nodeName))
            using (TemporaryEnvironmentVariable.Create(kubernetesPodName, podName))
            using (TemporaryEnvironmentVariable.Create(kubernetesNamespace, @namespace))
            {
                // Act
                logger.Information("This log event should be enriched with Kubernetes information");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            
            ContainsLogProperty(logEvent, kubernetesNodeName, nodeName);
            ContainsLogProperty(logEvent, kubernetesPodName, podName);
            ContainsLogProperty(logEvent, kubernetesNamespace, @namespace);
        }

        private static void ContainsLogProperty(LogEvent logEvent, string name, string expectedValue)
        {
            (string key, LogEventPropertyValue actual) = 
                Assert.Single(logEvent.Properties, prop => prop.Key == name);
            
            string actualValue = actual.ToString().Trim('\"');
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
