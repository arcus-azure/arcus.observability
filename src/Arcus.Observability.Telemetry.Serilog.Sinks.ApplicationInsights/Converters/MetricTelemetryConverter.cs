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
            Guard.NotNull(logEvent, nameof(logEvent), "Requires a Serilog log event to create an Azure Application Insights Metric telemetry instance");
            Guard.NotNull(logEvent.Properties, nameof(logEvent), "Requires a Serilog event with a set of properties to create an Azure Application Insights Metric telemetry instance");

            StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.MetricTracking.MetricLogEntry);
            string metricName = logEntry.Properties.GetAsRawString(nameof(MetricLogEntry.MetricName));
            double metricValue = logEntry.Properties.GetAsDouble(nameof(MetricLogEntry.MetricValue));
            DateTimeOffset timestamp = logEntry.Properties.GetAsDateTimeOffset(nameof(MetricLogEntry.Timestamp));
            IDictionary<string, string> context = logEntry.Properties.GetAsDictionary(nameof(MetricLogEntry.Context));
            
            var metricTelemetry = new MetricTelemetry(metricName, metricValue)
            {
                Timestamp = timestamp
            };

            metricTelemetry.Properties.AddRange(context);
            return metricTelemetry;
        }
    }
}