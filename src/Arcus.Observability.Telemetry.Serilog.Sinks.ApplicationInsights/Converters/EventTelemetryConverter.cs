using System;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="EventTelemetry"/> instance.
    /// </summary>
    public class EventTelemetryConverter : CustomTelemetryConverter<EventTelemetry>
    {
        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override EventTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var eventName = logEvent.Properties.GetAsRawString(ContextProperties.EventTracking.EventName);
            var eventContext = logEvent.Properties.GetAsDictionary(ContextProperties.EventTracking.EventContext);

            var eventTelemetry = new EventTelemetry(eventName);

            foreach (var contextProperty in eventContext)
            {
                var value = contextProperty.Value.ToDecentString();
                eventTelemetry.Properties.Add(contextProperty.Key.ToDecentString(), value);
            }

            return eventTelemetry;
        }

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            logEvent.RemovePropertyIfPresent(ContextProperties.EventTracking.EventName);
            logEvent.RemovePropertyIfPresent(ContextProperties.EventTracking.EventContext);
        }
    }
}
