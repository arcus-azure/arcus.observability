using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using EventLogEntry = Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Logging.EventLogEntry;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="EventTelemetry"/> instance.
    /// </summary>
    public class EventTelemetryConverter : CustomTelemetryConverter<EventTelemetry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to influence the behavior of the Application Insights Serilog sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public EventTelemetryConverter(ApplicationInsightsSinkOptions options) : base(options)
        {
        }

        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override EventTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

            StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.EventTracking.EventLogEntry);
            string eventName = logEntry.Properties.GetAsRawString(nameof(EventLogEntry.EventName));
            IDictionary<string, string> context = logEntry.Properties.GetAsDictionary(nameof(EventLogEntry.Context));

            var eventTelemetry = new EventTelemetry(eventName);
            eventTelemetry.Properties.AddRange(context);

            return eventTelemetry;
        }
    }
}
