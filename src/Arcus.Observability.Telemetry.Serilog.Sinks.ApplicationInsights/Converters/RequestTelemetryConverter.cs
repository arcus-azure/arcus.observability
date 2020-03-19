using System;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public class RequestTelemetryConverter : CustomTelemetryConverter<RequestTelemetry>
    {
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
