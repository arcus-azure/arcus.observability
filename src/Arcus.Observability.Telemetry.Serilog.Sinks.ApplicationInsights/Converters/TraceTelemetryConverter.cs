using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using ApplicationInsightsSink = Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public class TraceTelemetryConverter : ApplicationInsightsSink.TelemetryConverters.TraceTelemetryConverter
    {
        private readonly CloudContextConverter _cloudContextConverter = new CloudContextConverter();

        /// <inheritdoc />
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            foreach (var telemetry in base.Convert(logEvent, formatProvider))
            {
                _cloudContextConverter.EnrichWithAppInfo(logEvent, telemetry);

                yield return telemetry;
            }
        }
    }
}
