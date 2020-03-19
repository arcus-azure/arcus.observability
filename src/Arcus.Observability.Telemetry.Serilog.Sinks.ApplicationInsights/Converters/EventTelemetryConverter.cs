using System;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public class EventTelemetryConverter : CustomTelemetryConverter<EventTelemetry>
    {
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

        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            logEvent.RemovePropertyIfPresent(ContextProperties.EventTracking.EventName);
            logEvent.RemovePropertyIfPresent(ContextProperties.EventTracking.EventContext);
        }
    }
}
