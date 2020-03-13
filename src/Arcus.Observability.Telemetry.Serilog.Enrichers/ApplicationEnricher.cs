using GuardNet;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enrichment on log events with the application role concerning the component and instance name.
    /// </summary>
    public class ApplicationEnricher : ILogEventEnricher
    {
        private const string ComponentName = "ComponentName";

        private readonly string _componentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEnricher"/> class.
        /// </summary>
        /// <param name="componentName">The name of the application component.</param>
        public ApplicationEnricher(string componentName)
        {
            Guard.NotNullOrWhitespace(componentName, nameof(componentName), "Application component name cannot be blank");

            _componentValue = componentName;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            EnrichComponentName(ComponentName, _componentValue, logEvent, propertyFactory);
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
