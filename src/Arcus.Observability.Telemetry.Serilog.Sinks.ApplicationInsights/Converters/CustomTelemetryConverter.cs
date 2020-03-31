using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a custom typed conversion to a series of <see cref="ITelemetry"/> instances.
    /// </summary>
    /// <typeparam name="TEntry">The concrete <see cref="ITelemetry"/> implementation.</typeparam>
    public abstract class CustomTelemetryConverter<TEntry> : TelemetryConverterBase
        where TEntry : ITelemetry, ISupportProperties
    {
        private readonly CloudContextConverter _cloudContextConverter = new CloudContextConverter();

        /// <summary>
        ///     Convert the given <paramref name="logEvent"/> to a series of <see cref="ITelemetry"/> instances.
        /// </summary>
        /// <param name="logEvent">The event containing all relevant information for an <see cref="ITelemetry"/> instance.</param>
        /// <param name="formatProvider">The instance to control formatting.</param>
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            TEntry telemetryEntry = CreateTelemetryEntry(logEvent, formatProvider);

            AssignTelemetryContextProperties(logEvent, telemetryEntry);
            _cloudContextConverter.EnrichWithAppInfo(logEvent, telemetryEntry);
            ForwardPropertiesToTelemetryProperties(logEvent, telemetryEntry, formatProvider);
            RemoveIntermediaryProperties(logEvent);

            return new List<ITelemetry> {telemetryEntry};
        }

        /// <summary>
        /// Project the <see cref="LogEvent.Properties"/> to the properties of the given <paramref name="telemetry"/>.
        /// </summary>
        /// <param name="logEvent">The log event to extract the properties from.</param>
        /// <param name="telemetry">The destination telemetry instance to add the properties to.</param>
        protected void AssignTelemetryContextProperties(LogEvent logEvent, ISupportProperties telemetry)
        {
            var eventContext = logEvent.Properties.GetAsDictionary(ContextProperties.EventTracking.EventContext);

            foreach (KeyValuePair<ScalarValue, LogEventPropertyValue> contextProperty in eventContext)
            {
                var value = contextProperty.Value.ToDecentString();
                telemetry.Properties.Add(contextProperty.Key.ToDecentString(), value);
            }
        }

        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected abstract TEntry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider);

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected virtual void RemoveIntermediaryProperties(LogEvent logEvent)
        {
        }
    }
}