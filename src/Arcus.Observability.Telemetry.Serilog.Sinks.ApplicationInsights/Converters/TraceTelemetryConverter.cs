using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="TraceTelemetry"/> instance.
    /// </summary>
    public class TraceTelemetryConverter : global::Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter
    {
        private readonly OperationContextConverter _operationContextConverter;
        private readonly CloudContextConverter _cloudContextConverter = new CloudContextConverter();

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to influence the behavior of the Application Insights Serilog sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public TraceTelemetryConverter(ApplicationInsightsSinkOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _operationContextConverter = new OperationContextConverter(options);
        }

        /// <summary>
        ///     Convert the given <paramref name="logEvent"/> to a series of <see cref="ITelemetry"/> instances.
        /// </summary>
        /// <param name="logEvent">The event containing all relevant information for an <see cref="ITelemetry"/> instance.</param>
        /// <param name="formatProvider">The instance to control formatting.</param>
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

            foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
            {
                _cloudContextConverter.EnrichWithAppInfo(logEvent, telemetry);

                var traceTelemetry = telemetry as TraceTelemetry;
                _operationContextConverter.EnrichWithCorrelationInfo(traceTelemetry);

                yield return traceTelemetry;
            }
        }
    }
}
