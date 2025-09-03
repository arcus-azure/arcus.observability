using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
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
        /// Initializes a new instance of the <see cref="MetricTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to influence the behavior of the Application Insights Serilog sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public MetricTelemetryConverter(ApplicationInsightsSinkOptions options) : base(options)
        {
        }

        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override MetricTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

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