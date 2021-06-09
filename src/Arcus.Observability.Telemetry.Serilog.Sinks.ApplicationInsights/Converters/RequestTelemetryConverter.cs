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
            Guard.NotNull(logEvent, nameof(logEvent), "Requires a Serilog log event to create an Azure Application Insights Request telemetry instance");
            Guard.NotNull(logEvent.Properties, nameof(logEvent), "Requires a Serilog event with a set of properties to create an Azure Application Insights Request telemetry instance");
            
            StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.RequestTracking.RequestLogEntry);
            string requestMethod = logEntry.Properties.GetAsRawString(nameof(RequestLogEntry.RequestMethod));
            string requestHost = logEntry.Properties.GetAsRawString(nameof(RequestLogEntry.RequestHost));
            string requestUri = logEntry.Properties.GetAsRawString(nameof(RequestLogEntry.RequestUri));
            string responseStatusCode = logEntry.Properties.GetAsRawString(nameof(RequestLogEntry.ResponseStatusCode));
            TimeSpan requestDuration = logEntry.Properties.GetAsTimeSpan(nameof(RequestLogEntry.RequestDuration));
            DateTimeOffset requestTime = logEntry.Properties.GetAsDateTimeOffset(nameof(RequestLogEntry.RequestTime));
            IDictionary<string, string> context = logEntry.Properties.GetAsDictionary(nameof(RequestLogEntry.Context));

            string operationId = logEvent.Properties.GetAsRawString(ContextProperties.Correlation.OperationId);

            var requestName = $"{requestMethod} {requestUri}";
            bool isSuccessfulRequest = DetermineRequestOutcome(responseStatusCode);
            var url = new Uri($"{requestHost}{requestUri}");

            var requestTelemetry = new RequestTelemetry(requestName, requestTime, requestDuration, responseStatusCode, isSuccessfulRequest)
            {
                Id = operationId,
                Url = url,
            };

            requestTelemetry.Properties.AddRange(context);
            return requestTelemetry;
        }

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            Guard.NotNull(logEvent, nameof(logEvent), "Requires a Serilog log event to remove the intermediary Azure Application Insights Request telemetry properties");
            logEvent.RemovePropertyIfPresent(ContextProperties.RequestTracking.RequestLogEntry);
        }

        private static bool DetermineRequestOutcome(string rawResponseStatusCode)
        {
            var statusCode = int.Parse(rawResponseStatusCode);

            return statusCode >= 200 && statusCode < 300;
        }
    }
}
