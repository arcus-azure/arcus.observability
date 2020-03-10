using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public abstract class CustomTelemetryConverter<TEntry> : TelemetryConverterBase
        where TEntry : ITelemetry, ISupportProperties
    {
        private readonly CloudContextConverter _cloudContextConverter = new CloudContextConverter();

        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var telemetryEntry = CreateTelemetryEntry(logEvent, formatProvider);

            _cloudContextConverter.EnrichWithAppInfo(logEvent, telemetryEntry);
            RemoveIntermediaryProperties(logEvent);
            ForwardPropertiesToTelemetryProperties(logEvent, telemetryEntry, formatProvider);

            return new List<ITelemetry> {telemetryEntry};
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