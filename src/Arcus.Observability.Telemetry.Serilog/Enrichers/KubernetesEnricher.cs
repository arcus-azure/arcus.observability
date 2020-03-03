using System;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
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

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            EnrichEnvironmentVariable(NodeNameVariable, NodeNameProperty, logEvent, propertyFactory);
            EnrichEnvironmentVariable(PodNameVariable, PodNameProperty, logEvent, propertyFactory);
            EnrichEnvironmentVariable(NamespaceVariable, NamespaceProperty, logEvent, propertyFactory);
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
