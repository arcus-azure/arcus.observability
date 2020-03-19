using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters.Dependencies;
using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public class ApplicationInsightsTelemetryConverter : TelemetryConverterBase
    {
        private readonly TraceTelemetryConverter _traceTelemetryConverter = new TraceTelemetryConverter();
        private readonly EventTelemetryConverter _eventTelemetryConverter = new EventTelemetryConverter();
        private readonly RequestTelemetryConverter _requestTelemetryConverter = new RequestTelemetryConverter();

        private readonly HttpDependencyTelemetryConverter _httpDependencyTelemetryConverter =
            new HttpDependencyTelemetryConverter();

        private readonly SqlDependencyTelemetryConverter _sqlDependencyTelemetryConverter =
            new SqlDependencyTelemetryConverter();

        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent.MessageTemplate.Text.StartsWith(MessagePrefixes.RequestViaHttp))
            {
                return _requestTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.MessageTemplate.Text.StartsWith(MessagePrefixes.DependencyViaHttp))
            {
                return _httpDependencyTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.MessageTemplate.Text.StartsWith(MessagePrefixes.DependencyViaSql))
            {
                return _sqlDependencyTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.MessageTemplate.Text.StartsWith(MessagePrefixes.Event))
            {
                return _eventTelemetryConverter.Convert(logEvent, formatProvider);
            }

            return _traceTelemetryConverter.Convert(logEvent, formatProvider);
        }

        /// <summary>
        ///     Creates an instance of the converter
        /// </summary>
        public static ApplicationInsightsTelemetryConverter Create()
        {
            return new ApplicationInsightsTelemetryConverter();
        }
    }
}