using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a general conversion from Serilog <see cref="LogEvent"/> instances to Application Insights <see cref="ITelemetry"/> instances.
    /// </summary>
    public class ApplicationInsightsTelemetryConverter : TelemetryConverterBase
    {
        private readonly RequestTelemetryConverter _requestTelemetryConverter;
        private readonly ExceptionTelemetryConverter _exceptionTelemetryConverter;
        private readonly TraceTelemetryConverter _traceTelemetryConverter;
        private readonly EventTelemetryConverter _eventTelemetryConverter;
        private readonly MetricTelemetryConverter _metricTelemetryConverter;
        private readonly DependencyTelemetryConverter _dependencyTelemetryConverter;

        private ApplicationInsightsTelemetryConverter(ApplicationInsightsSinkOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            _requestTelemetryConverter = new RequestTelemetryConverter(options);
            _exceptionTelemetryConverter = new ExceptionTelemetryConverter(options);
            _traceTelemetryConverter = new TraceTelemetryConverter(options);
            _eventTelemetryConverter = new EventTelemetryConverter(options);
            _metricTelemetryConverter = new MetricTelemetryConverter(options);
            _dependencyTelemetryConverter = new DependencyTelemetryConverter(options);
        }

        /// <summary>
        ///     Convert the given <paramref name="logEvent"/> to a series of <see cref="ITelemetry"/> instances.
        /// </summary>
        /// <param name="logEvent">The event containing all relevant information for an <see cref="ITelemetry"/> instance.</param>
        /// <param name="formatProvider">The instance to control formatting.</param>
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent.Exception != null)
            {
                return _exceptionTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.Properties.ContainsKey(ContextProperties.RequestTracking.RequestLogEntry))
            {
                return _requestTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.Properties.ContainsKey(ContextProperties.DependencyTracking.DependencyLogEntry))
            {
                return _dependencyTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.Properties.ContainsKey(ContextProperties.EventTracking.EventLogEntry))
            {
                return _eventTelemetryConverter.Convert(logEvent, formatProvider);
            }

            if (logEvent.Properties.ContainsKey(ContextProperties.MetricTracking.MetricLogEntry))
            {
                return _metricTelemetryConverter.Convert(logEvent, formatProvider);
            }

            return _traceTelemetryConverter.Convert(logEvent, formatProvider);
        }

        /// <summary>
        ///     Creates an instance of the converter
        /// </summary>
        public static ApplicationInsightsTelemetryConverter Create()
        {
            return Create(new ApplicationInsightsSinkOptions());
        }

        /// <summary>
        /// Create an instance of the <see cref="ApplicationInsightsTelemetryConverter"/> class.
        /// </summary>
        /// <param name="options">The optional user-defined configuration options to influence the tracking behavior to Azure Application Insights.</param>
        public static ApplicationInsightsTelemetryConverter Create(ApplicationInsightsSinkOptions options)
        {
            return new ApplicationInsightsTelemetryConverter(options ?? new ApplicationInsightsSinkOptions());
        }
    }
}