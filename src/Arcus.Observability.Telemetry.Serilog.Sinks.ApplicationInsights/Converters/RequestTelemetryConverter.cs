using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
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
        private ApplicationInsightsSinkRequestOptions _options;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTelemetryConverter" /> class.
        /// </summary>
        public RequestTelemetryConverter()
            : this(new ApplicationInsightsSinkRequestOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to tracking requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options" /> is <c>null</c>.</exception>
        public RequestTelemetryConverter(ApplicationInsightsSinkRequestOptions options)
        {
            Guard.NotNull(options, nameof(options), "Requires a set of user-configurable options to influence the behavior of how requests are tracked");
            _options = options;
        }
        
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
            string operationName = logEntry.Properties.GetAsRawString(nameof(RequestLogEntry.OperationName));
            TimeSpan requestDuration = logEntry.Properties.GetAsTimeSpan(nameof(RequestLogEntry.RequestDuration));
            DateTimeOffset requestTime = logEntry.Properties.GetAsDateTimeOffset(nameof(RequestLogEntry.RequestTime));
            IDictionary<string, string> context = logEntry.Properties.GetAsDictionary(nameof(RequestLogEntry.Context));

            string id = _options.GenerateId();

            var requestName = $"{requestMethod} {requestUri}";
            bool isSuccessfulRequest = DetermineRequestOutcome(responseStatusCode);
            var url = new Uri($"{requestHost}{requestUri}");

            var requestTelemetry = new RequestTelemetry(requestName, requestTime, requestDuration, responseStatusCode, isSuccessfulRequest)
            {
                Id = id,
                Url = url
            };

            if (!operationName.StartsWith(requestMethod))
            {
                requestTelemetry.Context.Operation.Name = $"{requestMethod} {operationName}";
            }
            else
            {
                requestTelemetry.Context.Operation.Name = operationName;
            }

            requestTelemetry.Properties.AddRange(context);
            return requestTelemetry;
        }

        private static bool DetermineRequestOutcome(string rawResponseStatusCode)
        {
            var statusCode = int.Parse(rawResponseStatusCode);

            return statusCode >= 200 && statusCode < 300;
        }
    }
}
