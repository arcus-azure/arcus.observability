using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using ApplicationInsightsSink = Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="TraceTelemetry"/> instance.
    /// </summary>
    public class TraceTelemetryConverter : ApplicationInsightsSink.TelemetryConverters.TraceTelemetryConverter
    {
        private readonly OperationContextConverter _operationContextConverter = new OperationContextConverter();
        private readonly CloudContextConverter _cloudContextConverter = new CloudContextConverter();

        /// <summary>
        ///     Convert the given <paramref name="logEvent"/> to a series of <see cref="ITelemetry"/> instances.
        /// </summary>
        /// <param name="logEvent">The event containing all relevant information for an <see cref="ITelemetry"/> instance.</param>
        /// <param name="formatProvider">The instance to control formatting.</param>
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
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
