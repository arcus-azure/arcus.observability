using System;
using Arcus.Observability.Telemetry.Core;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enrichment on log events that automatically adds Kubernetes information from the environment.
    /// </summary>
    /// <remarks>
    /// The default Kubernetes environment variable property names; <c>KUBERNETES_NODE_NAME</c>, <c>KUBERNETES_POD_NAME</c> AND<c>KUBERNETES_NAMESPACE</c> are used to fetch the Kubernetes information.
    /// </remarks>
    public class KubernetesEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Gets the default Kubernetes node name environment variable name.
        /// </summary>
        public const string NodeNameVariable = "KUBERNETES_NODE_NAME";

        /// <summary>
        /// Gets the default Kubernetes pod name environment variable name.
        /// </summary>
        public const string PodNameVariable = "KUBERNETES_POD_NAME";

        /// <summary>
        /// Gets the default Kubernetes namespace environment variable name.
        /// </summary>
        public const string NamespaceVariable = "KUBERNETES_NAMESPACE";

        private readonly string _nodeNamePropertyName, _podNamePropertyName, _namespacePropertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesEnricher"/> class.
        /// </summary>
        public KubernetesEnricher()
            : this(ContextProperties.Kubernetes.NodeName, ContextProperties.Kubernetes.PodName, ContextProperties.Kubernetes.Namespace)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesEnricher"/> class.
        /// </summary>
        /// <param name="nodeNamePropertyName">The name of the property as it's logged to enrich the log event with the Kubernetes node name.</param>
        /// <param name="podNamePropertyName">The name of the property as it's logged to enrich the log event with the Kubernetes pod name.</param>
        /// <param name="namespacePropertyName">The name of the property as it's logged to enrich the log event with the Kubernetes namespace.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="nodeNamePropertyName"/>, <paramref name="podNamePropertyName"/>, or <paramref name="namespacePropertyName"/> is blank.</exception>
        public KubernetesEnricher(string nodeNamePropertyName, string podNamePropertyName, string namespacePropertyName)
        {
            if (string.IsNullOrWhiteSpace(nodeNamePropertyName))
            {
                throw new ArgumentException("Requires a non-blank property name to enrich the log event with the Kubernetes node name", nameof(nodeNamePropertyName));
            }

            if (string.IsNullOrWhiteSpace(podNamePropertyName))
            {
                throw new ArgumentException("Requires a non-blank property name to enrich the log event with the Kubernetes pod name", nameof(podNamePropertyName));
            }

            if (string.IsNullOrWhiteSpace(namespacePropertyName))
            {
                throw new ArgumentException("Requires a non-blank property name to enrich the log event with the Kubernetes namespace name", nameof(namespacePropertyName));
            }

            _nodeNamePropertyName = nodeNamePropertyName;
            _podNamePropertyName = podNamePropertyName;
            _namespacePropertyName = namespacePropertyName;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            EnrichEnvironmentVariable(NodeNameVariable, _nodeNamePropertyName, logEvent, propertyFactory);
            EnrichEnvironmentVariable(PodNameVariable, _podNamePropertyName, logEvent, propertyFactory);
            EnrichEnvironmentVariable(NamespaceVariable, _namespacePropertyName, logEvent, propertyFactory);
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
