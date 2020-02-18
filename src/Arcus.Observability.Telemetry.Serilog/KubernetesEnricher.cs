using System;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog
{
    /// <summary>
    /// Enrichment on log events that automatically adds Kubernetes information from the environment.
    /// </summary>
    public class KubernetesEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            EnrichEnvironmentVariable("KUBERNETES_NODE_NAME", logEvent, propertyFactory);
            EnrichEnvironmentVariable("KUBERNETES_POD_NAME", logEvent, propertyFactory);
            EnrichEnvironmentVariable("KUBERNETES_NAMESPACE", logEvent, propertyFactory);
        }

        private static void EnrichEnvironmentVariable(string name, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            if (!String.IsNullOrWhiteSpace(value))
            {
                LogEventProperty property = propertyFactory.CreateProperty(name, value);
                logEvent.AddPropertyIfAbsent(property);
            }
        }
    }
}
