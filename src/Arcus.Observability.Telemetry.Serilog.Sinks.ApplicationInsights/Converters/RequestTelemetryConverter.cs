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
        private readonly ApplicationInsightsSinkRequestOptions _options;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTelemetryConverter" /> class.
        /// </summary>
        [Obsolete("Use the constructor overload with the Application Insights options instead")]
        public RequestTelemetryConverter()
            : this(new ApplicationInsightsSinkRequestOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to tracking requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options" /> is <c>null</c>.</exception>
        [Obsolete("Use the constructor overload with the Application Insights options instead")]
        public RequestTelemetryConverter(ApplicationInsightsSinkRequestOptions options)
        {
            Guard.NotNull(options, nameof(options), "Requires a set of user-configurable options to influence the behavior of how requests are tracked");
            _options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to influence the behavior of the Application Insights Serilog sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public RequestTelemetryConverter(ApplicationInsightsSinkOptions options) : base(options)
        {
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
            var sourceSystem = logEntry.Properties.GetAsObject<RequestSourceSystem>(nameof(RequestLogEntry.SourceSystem));

            string sourceName = DetermineSourceName(sourceSystem, requestMethod, requestUri, operationName);
            bool isSuccessfulRequest = DetermineRequestOutcome(responseStatusCode);
            Uri url = DetermineUrl(sourceSystem, requestHost, requestUri);
            string source = DetermineRequestSource(sourceSystem, context);
            string requestOperationName = DetermineRequestOperationName(sourceSystem, requestMethod, operationName);

            var requestTelemetry = new RequestTelemetry(sourceName, requestTime, requestDuration, responseStatusCode, isSuccessfulRequest)
            {
                Url = url,
                Source = source,
                Context = { Operation = { Name = requestOperationName } }
            };

            if (_options != null)
            {
                requestTelemetry.Id = _options.GenerateId();
            }

            requestTelemetry.Properties.AddRange(context);
            return requestTelemetry;
        }

        private static string DetermineSourceName(RequestSourceSystem sourceSystem, string requestMethod, string requestUri, string operationName)
        {
            if (sourceSystem is RequestSourceSystem.Http)
            {
                var requestName = $"{requestMethod} {requestUri}";
                return requestName;
            }

            return operationName;
        }

        private static Uri DetermineUrl(RequestSourceSystem sourceSystem, string requestHost, string requestUri)
        {
            if (sourceSystem is RequestSourceSystem.Http)
            {
                var url = new Uri($"{requestHost}{requestUri}");
                return url;
            }

            return null;
        }

        private static string DetermineRequestSource(RequestSourceSystem sourceSystem, IDictionary<string, string> context)
        {
            if (sourceSystem is RequestSourceSystem.AzureServiceBus)
            {
                string entityName = context[ContextProperties.RequestTracking.ServiceBus.EntityName];
                string namespaceEndpoint = context[ContextProperties.RequestTracking.ServiceBus.Endpoint];
                
                return $"type:Azure Service Bus | name:{entityName} | endpoint:sb://{namespaceEndpoint}/";
            }

            if (sourceSystem is RequestSourceSystem.EventHubs)
            {
                string name = context[ContextProperties.RequestTracking.EventHubs.Name];
                string namespaceEndpoint = context[ContextProperties.RequestTracking.EventHubs.Namespace];
                
                return $"{namespaceEndpoint}.servicebus.windows.net/{name}";
            }

            return null;
        }

        private static bool DetermineRequestOutcome(string rawResponseStatusCode)
        {
            var statusCode = int.Parse(rawResponseStatusCode);

            return statusCode >= 200 && statusCode < 300;
        }

        private static string DetermineRequestOperationName(RequestSourceSystem sourceSystem, string requestMethod, string operationName)
        {
            if (sourceSystem is RequestSourceSystem.Http && !operationName.StartsWith(requestMethod))
            {
                return $"{requestMethod} {operationName}";
            }

            return operationName;
        }
    }
}
