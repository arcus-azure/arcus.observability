using GuardNet;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog
{
    /// <summary>
    /// Enrichment on log events with the application role concerning the component and instance name.
    /// </summary>
    public class ApplicationEnricher : ILogEventEnricher
    {
        private const string ComponentName = "ComponentName",
                             InstanceName = "InstanceName";

        private readonly string _componentValue;
        private readonly ApplicationInstance _applicationInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEnricher"/> class.
        /// </summary>
        /// <param name="componentName">The name of the application component.</param>
        /// <param name="applicationInstance">The location where the application instance should be retrieved from other log properties.</param>
        public ApplicationEnricher(string componentName, ApplicationInstance applicationInstance)
        {
            Guard.NotNullOrWhitespace(componentName, nameof(componentName));

            _componentValue = componentName;
            _applicationInstance = applicationInstance;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            EnrichComponentName(ComponentName, _componentValue, logEvent, propertyFactory);
            EnrichInstanceName(logEvent);
        }

        private void EnrichInstanceName(LogEvent logEvent)
        {
            if (TryGetInstanceName(logEvent, out LogEventPropertyValue value))
            {
                var property = new LogEventProperty(InstanceName, value);
                logEvent.AddPropertyIfAbsent(property);
            }
        }

        private bool TryGetInstanceName(LogEvent logEvent, out LogEventPropertyValue value)
        {
            switch (_applicationInstance)
            {
                case ApplicationInstance.MachineName:
                    return logEvent.Properties.TryGetValue("MachineName", out value);
                case ApplicationInstance.PodName:
                    return logEvent.Properties.TryGetValue(KubernetesEnricher.PodNameProperty, out value);
                default:
                    value = null;
                    return false;
            }
        }

        private static void EnrichComponentName(
            string name,
            string value,
            LogEvent logEvent,
            ILogEventPropertyFactory propertyFactory)
        {
            LogEventProperty property = propertyFactory.CreateProperty(name, value);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
