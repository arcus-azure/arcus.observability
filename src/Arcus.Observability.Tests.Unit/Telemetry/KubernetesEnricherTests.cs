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
            ILogger logger = new LoggerConfiguration()
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
            
            Assert.True(logEvent.ContainsProperty("NodeName", nodeName), "Log event should contain node name property");
            Assert.True(logEvent.ContainsProperty("PodName", podName), "Log event should contain pod name property");
            Assert.True(logEvent.ContainsProperty("Namespace", @namespace), "Log event should contain namespace property");
        }

        [Fact]
        public void LogEventWithNodeNameProperty_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expectedNodeName = $"node-{Guid.NewGuid()}";
            string ignoredNodeName = $"node-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.Create("KUBERNETES_NODE_NAME", ignoredNodeName))
            {
                // Act
                logger.Information("This log even already has a Kubernetes NodeName {NodeName}", expectedNodeName);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty("NodeName", expectedNodeName), "Log event should contain node name property");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == "PodName");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == "Namespace");
        }

        [Fact]
        public void LogEventWithPodNameProperty_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expectedPodName = $"pod-{Guid.NewGuid()}";
            string ignoredPodName = $"pod-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.Create("KUBERNETES_POD_NAME", ignoredPodName))
            {
                // Act
                logger.Information("This log even already has a Kubernetes PodName {PodName}", expectedPodName);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty("PodName", expectedPodName), "Log event should contain pod name property");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == "NodeName");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == "Namespace");
        }

        [Fact]
        public void LogEventWithNamespaceProperty_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expectedNamespace = $"namespace-{Guid.NewGuid()}";
            string ignoredNamespace = $"namespace-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.Create("KUBERNETES_NAMESPACE", ignoredNamespace))
            {
                // Act
                logger.Information("This log even already has a Kubernetes Namespace {Namespace}", expectedNamespace);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty("Namespace", expectedNamespace), "Log event should contain namespace property");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == "NodeName");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == "PodName");
        }
    }
}
