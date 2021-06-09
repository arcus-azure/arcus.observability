using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using GuardNet;
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
            Guard.NotNull(logEvent, nameof(logEvent), "Requires a Serilog log event to create an Azure Application Insights Event telemetry instance");
            Guard.NotNull(logEvent.Properties, nameof(logEvent), "Requires a Serilog event with a set of properties to create an Azure Application Insights Event telemetry instance");

            StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.EventTracking.EventLogEntry);
            string eventName = logEntry.Properties.GetAsRawString(nameof(EventLogEntry.EventName));
            IDictionary<string, string> context = logEntry.Properties.GetAsDictionary(nameof(EventLogEntry.Context));
            
            var eventTelemetry = new EventTelemetry(eventName);
            eventTelemetry.Properties.AddRange(context);
            
            return eventTelemetry;
        }

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            Guard.NotNull(logEvent, nameof(logEvent), "Requires a Serilog log event to remove the intermediary Azure Application Insights Event telemetry properties");
            logEvent.RemovePropertyIfPresent(ContextProperties.EventTracking.EventLogEntry);
        }
    }
}
