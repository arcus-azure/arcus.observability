using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters 
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to a Application Insights <see cref="MetricTelemetry"/> instance.
    /// </summary>
    public class MetricTelemetryConverter : CustomTelemetryConverter<MetricTelemetry>
    {
        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override MetricTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var metricName = logEvent.Properties.GetAsRawString(ContextProperties.MetricTracking.MetricName);
            var metricValue = logEvent.Properties.GetAsDouble(ContextProperties.MetricTracking.MetricValue);
            var timestamp = logEvent.Properties.GetAsDateTimeOffset(ContextProperties.MetricTracking.Timestamp);

            var metricTelemetry = new MetricTelemetry(metricName, metricValue)
            {
                Timestamp = timestamp
            };

            return metricTelemetry;
        }

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            logEvent.RemovePropertyIfPresent(ContextProperties.MetricTracking.MetricName);
            logEvent.RemovePropertyIfPresent(ContextProperties.MetricTracking.MetricValue);
            logEvent.RemovePropertyIfPresent(ContextProperties.MetricTracking.Timestamp);
        }
    }
}