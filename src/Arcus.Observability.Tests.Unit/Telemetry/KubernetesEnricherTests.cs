using System;
using System.Collections.Generic;
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
            
            ContainsLogProperty(logEvent, kubernetesNodeName, nodeName);
            ContainsLogProperty(logEvent, kubernetesPodName, podName);
            ContainsLogProperty(logEvent, kubernetesNamespace, @namespace);
        }

        [Fact]
        public void LogEvent_WithCustomEnvironmentVariablesWithinKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            const string kubernetesNodeName = "MY_KUBERNETES_NODE_NAME",
                         kubernetesPodName = "MY_KUBERNETES_POD_NAME",
                         kubernetesNamespace = "MY_KUBERNETES_NAMESPACE";

            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With(new KubernetesEnricher(kubernetesNodeName, kubernetesPodName, kubernetesNamespace))
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

        [Fact]
        public void LogEvent_WithCustomEnvironmentVariablesAndLogPropertyNamesWithinKubernetesEnricher_HasEnvironmentInformation()
        {
            // Arrange
            const string kubernetesNodeNameEnvVar = "MY_KUBERNETES_NODE_NAME",
                         kubernetesPodNameEnvVar = "MY_KUBERNETES_POD_NAME",
                         kubernetesNamespaceEnvVar = "MY_KUBERNETES_NAMESPACE";

            const string kubernetesNodeNamePropName = "MyKubernetesNodeName",
                         kubernetesPodNamePropName = "MyKubernetesPodName",
                         kubernetesNamespacePropName = "MyKubernetesNamespace";

            string nodeName = $"node-{Guid.NewGuid()}";
            string podName = $"pod-{Guid.NewGuid()}";
            string @namespace = $"namespace-{Guid.NewGuid()}";

            var spy = new InMemoryLogSink();
            var kubernetesEnvironmentToLogProperty = new Dictionary<string, string>
            {
                [kubernetesNodeNameEnvVar] = kubernetesNodeNamePropName,
                [kubernetesPodNameEnvVar] = kubernetesPodNamePropName, 
                [kubernetesNamespaceEnvVar] = kubernetesNamespacePropName
            };
            ILogger logger = new LoggerConfiguration()
                .Enrich.With(new KubernetesEnricher(kubernetesEnvironmentToLogProperty))
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.Create(kubernetesNodeNameEnvVar, nodeName))
            using (TemporaryEnvironmentVariable.Create(kubernetesPodNameEnvVar, podName))
            using (TemporaryEnvironmentVariable.Create(kubernetesNamespaceEnvVar, @namespace))
            {
                // Act
                logger.Information("This log event should be enriched with Kubernetes information");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            
            ContainsLogProperty(logEvent, kubernetesNodeNamePropName, nodeName);
            ContainsLogProperty(logEvent, kubernetesPodNamePropName, podName);
            ContainsLogProperty(logEvent, kubernetesNamespacePropName, @namespace);
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
