using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Arcus.Testing;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    [Trait("Category", "Integration")]
    public class KubernetesEnricherTests
    {
        private const string NodeNameVariable = "KUBERNETES_NODE_NAME",
                             PodNameVariable = "KUBERNETES_POD_NAME",
                             NamespaceVariable = "KUBERNETES_NAMESPACE";

        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesEnricherTests"/> class.
        /// </summary>
        public KubernetesEnricherTests(ITestOutputHelper outputWriter)
        {
            _logger = new XunitTestLogger(outputWriter);
        }

        [Fact]
        public void LogEvent_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.SetIfNotExists(NodeNameVariable, nodeName, _logger))
            using (TemporaryEnvironmentVariable.SetIfNotExists(PodNameVariable, podName, _logger))
            using (TemporaryEnvironmentVariable.SetIfNotExists(NamespaceVariable, @namespace, _logger))
            {
                // Act
                logger.Information("This log event should be enriched with Kubernetes information");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty(ContextProperties.Kubernetes.NodeName, nodeName), "Log event should contain node name property");
            Assert.True(logEvent.ContainsProperty(ContextProperties.Kubernetes.PodName, podName), "Log event should contain pod name property");
            Assert.True(logEvent.ContainsProperty(ContextProperties.Kubernetes.Namespace, @namespace), "Log event should contain namespace property");
        }

        [Fact]
        public void LogEventWithNodeNameProperty_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expectedNodeName = $"node-{Guid.NewGuid()}";
            string ignoredNodeName = $"node-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithKubernetesInfo()
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.SetIfNotExists(NodeNameVariable, ignoredNodeName, _logger))
            {
                // Act
                logger.Information("This log even already has a Kubernetes NodeName {NodeName}", expectedNodeName);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty(ContextProperties.Kubernetes.NodeName, expectedNodeName), "Log event should contain node name property");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == ContextProperties.Kubernetes.PodName);
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == ContextProperties.Kubernetes.Namespace);
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

            using (TemporaryEnvironmentVariable.SetIfNotExists(PodNameVariable, ignoredPodName, _logger))
            {
                // Act
                logger.Information("This log even already has a Kubernetes PodName {PodName}", expectedPodName);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty(ContextProperties.Kubernetes.PodName, expectedPodName), "Log event should contain pod name property");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == ContextProperties.Kubernetes.NodeName);
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == ContextProperties.Kubernetes.Namespace);
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

            using (TemporaryEnvironmentVariable.SetIfNotExists(NamespaceVariable, ignoredNamespace, _logger))
            {
                // Act
                logger.Information("This log even already has a Kubernetes Namespace {Namespace}", expectedNamespace);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);

            Assert.True(logEvent.ContainsProperty(ContextProperties.Kubernetes.Namespace, expectedNamespace), "Log event should contain namespace property");
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == ContextProperties.Kubernetes.NodeName);
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == ContextProperties.Kubernetes.PodName);
        }

        [Fact]
        public void LogEventWithCustomNodeNamePropertyName_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expected = $"node-{Guid.NewGuid()}";
            string propertyName = $"node-name-{Guid.NewGuid():N}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithKubernetesInfo(nodeNamePropertyName: propertyName)
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.SetIfNotExists(NodeNameVariable, expected, _logger))
            {
                // Act
                logger.Information("This log event should contain the custom configured Kubernetes property");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            Assert.True(logEvent.ContainsProperty(propertyName, expected), $"Log event contain property '{propertyName}' with value '{expected}'");
        }

        [Fact]
        public void LogEventWithCustomPodNamePropertyName_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expected = $"pod-{Guid.NewGuid()}";
            string propertyName = $"pod-name-{Guid.NewGuid():N}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithKubernetesInfo(podNamePropertyName: propertyName)
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.SetIfNotExists(PodNameVariable, expected, _logger))
            {
                // Act
                logger.Information("This log event should contain the custom configured Kubernetes property");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            Assert.True(logEvent.ContainsProperty(propertyName, expected), $"Log event contain property '{propertyName}' with value '{expected}'");
        }

        [Fact]
        public void LogEventWithCustomNamespacePropertyName_WithKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            string expected = $"namespace-{Guid.NewGuid()}";
            string propertyName = $"namespace-name-{Guid.NewGuid():N}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithKubernetesInfo(namespacePropertyName: propertyName)
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.SetIfNotExists(NamespaceVariable, expected, _logger))
            {
                // Act
                logger.Information("This log event should contain the custom configured Kubernetes property");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            Assert.True(logEvent.ContainsProperty(propertyName, expected), $"Log event contain property '{propertyName}' with value '{expected}'");
        }
    }
}
