using System;
using System.Collections.Generic;
using System.Linq;
using GuardNet;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog
{
    /// <summary>
    /// Enrichment on log events that automatically adds Kubernetes information from the environment.
    /// </summary>
    public class KubernetesEnricher : ILogEventEnricher
    {
        private const string NodeNameVariable = "KUBERNETES_NODE_NAME",
                             PodNameVariable = "KUBERNETES_POD_NAME",
                             NamespaceVariable = "KUBERNETES_NAMESPACE";

        private const string NodeNameProperty = "NodeName",
                             PodNameProperty = "PodName",
                             NamespaceProperty = "Namespace";

        private readonly IDictionary<string, string> _kubernetesEnvironmentToLogProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesEnricher"/> class.
        /// </summary>
        public KubernetesEnricher() : this(NodeNameVariable, PodNameVariable, NamespaceVariable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesEnricher"/> class.
        /// </summary>
        /// <param name="nodeNameVariableName">The name of the environment variable that specifies the Kubernetes node name.</param>
        /// <param name="podNameVariableName">The name of the environment variable that specifies the Kubernetes pod name.</param>
        /// <param name="namespaceVariableName">The name of the environment variable that specifies the Kubernetes namespace.</param>
        public KubernetesEnricher(string nodeNameVariableName, string podNameVariableName, string namespaceVariableName)
            : this(new Dictionary<string, string>
                   {
                       [nodeNameVariableName] = NodeNameProperty,
                       [podNameVariableName] = PodNameProperty,
                       [namespaceVariableName] = NamespaceProperty
                   })
        {
        }

        private KubernetesEnricher(IDictionary<string, string> kubernetesEnvironmentToLogProperty)
        {
            Guard.NotNull(kubernetesEnvironmentToLogProperty, nameof(kubernetesEnvironmentToLogProperty));
            Guard.For<ArgumentException>(
                () => kubernetesEnvironmentToLogProperty.Any(item => String.IsNullOrWhiteSpace(item.Key) || String.IsNullOrWhiteSpace(item.Value)),
                "Requires a set of non-blank key/value pairs to specify the environment variable that corresponds with a log property");

            _kubernetesEnvironmentToLogProperty = kubernetesEnvironmentToLogProperty;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (KeyValuePair<string, string> item in _kubernetesEnvironmentToLogProperty)
            {
                EnrichEnvironmentVariable(item.Key, item.Value, logEvent, propertyFactory);
            }
        }

        private static void EnrichEnvironmentVariable(
            string environmentVariableName,
            string logPropertyName,
            LogEvent logEvent,
            ILogEventPropertyFactory propertyFactory)
        {
            string value = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Process);
            if (!String.IsNullOrWhiteSpace(value))
            {
                LogEventProperty property = propertyFactory.CreateProperty(logPropertyName, value);
                logEvent.AddPropertyIfAbsent(property);
            }
        }
    }
}
