using System;
using GuardNet;
using Serilog.Core;
using Serilog.Enrichers;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enrichment on log events with the application role concerning the component and instance name.
    /// </summary>
    public class ApplicationEnricher : ILogEventEnricher
    {
        internal const string ComponentName = "ComponentName";

        private readonly ILogEventEnricher _machineNameEnricher = new MachineNameEnricher();
        private readonly string _componentValue, _propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEnricher"/> class.
        /// </summary>
        /// <param name="componentName">The name of the application component.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="componentName"/> is blank.</exception>
        public ApplicationEnricher(string componentName) : this(componentName, ComponentName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEnricher"/> class.
        /// </summary>
        /// <param name="componentName">The name of the application component.</param>
        /// <param name="propertyName">The name of the property to enrich the log event with the <paramref name="componentName"/>.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="componentName"/> or <paramref name="propertyName"/> is blank.</exception>
        public ApplicationEnricher(string componentName, string propertyName)
        {
            Guard.NotNullOrWhitespace(componentName, nameof(componentName), "Requires a non-blank application component name");
            Guard.NotNullOrWhitespace(propertyName, nameof(propertyName), "Requires a non-blank property name to enrich the log event with the component name");

            _componentValue = componentName;
            _propertyName = propertyName;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            LogEventProperty property = propertyFactory.CreateProperty(_propertyName, _componentValue);
            logEvent.AddPropertyIfAbsent(property);

            _machineNameEnricher.Enrich(logEvent, propertyFactory);
        }
    }
}
