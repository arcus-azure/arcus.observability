using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="RequestTelemetry"/> instance.
    /// </summary>
    public class RequestTelemetryConverter : CustomTelemetryConverter<RequestTelemetry>
    {
        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override RequestTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var requestMethod = logEvent.Properties.GetAsRawString(ContextProperties.RequestTracking.RequestMethod);
            var requestHost = logEvent.Properties.GetAsRawString(ContextProperties.RequestTracking.RequestHost);
            var requestUri = logEvent.Properties.GetAsRawString(ContextProperties.RequestTracking.RequestUri);
            var responseStatusCode = logEvent.Properties.GetAsRawString(ContextProperties.RequestTracking.ResponseStatusCode);
            var requestDuration = logEvent.Properties.GetAsTimeSpan(ContextProperties.RequestTracking.RequestDuration);
            var requestTime = logEvent.Properties.GetAsDateTimeOffset(ContextProperties.RequestTracking.RequestTime);
            var operationId = logEvent.Properties.GetAsRawString(ContextProperties.Correlation.OperationId);

            var requestName = $"{requestMethod} {requestUri}";
            var isSuccessfulRequest = DetermineRequestOutcome(responseStatusCode);
            var url = new Uri($"{requestHost}{requestUri}");

            var requestTelemetry = new RequestTelemetry(requestName, requestTime, requestDuration, responseStatusCode, isSuccessfulRequest)
            {
                Id = operationId,
                Url = url
            };

            return requestTelemetry;
        }

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.RequestMethod);
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.RequestHost);
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.RequestUri);
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.ResponseStatusCode);
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.RequestDuration);
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.RequestTime);
        }

        private static bool DetermineRequestOutcome(string rawResponseStatusCode)
        {
            var statusCode = int.Parse(rawResponseStatusCode);

            return statusCode >= 200 && statusCode < 300;
        }
    }
}
