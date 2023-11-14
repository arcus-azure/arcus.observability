using System;
using System.Collections.Generic;
using System.Linq;
using GuardNet;

namespace Arcus.Observability.Telemetry.Core.Logging
{
    /// <summary>
    /// Represents a HTTP request as a log entry.
    /// </summary>
    public class RequestLogEntry
    {
        private RequestLogEntry(
            string method,
            string host,
            string uri,
            string operationName,
            int statusCode,
            string customRequestSource,
            string requestTime,
            TimeSpan duration,
            IDictionary<string, object> context)
            : this(method, host, uri, operationName, statusCode, RequestSourceSystem.Custom, requestTime, duration, context)
        {
            CustomRequestSource = customRequestSource;
        }

        private RequestLogEntry(
            string method,
            string host,
            string uri,
            string operationName,
            int statusCode,
            RequestSourceSystem sourceSystem,
            string requestTime,
            TimeSpan duration,
            IDictionary<string, object> context)
        {
            Guard.For<ArgumentException>(() => host?.Contains(" ") is true, "Requires a HTTP request host name without whitespace");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank operation name");
            Guard.NotLessThan(statusCode, 100, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotGreaterThan(statusCode, 599, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            RequestMethod = method;
            RequestHost = host;
            RequestUri = uri;
            ResponseStatusCode = statusCode;
            RequestDuration = duration;
            OperationName = operationName;
            SourceSystem = sourceSystem;
            RequestTime = requestTime;
            Context = context;
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Request;
        }

        /// <summary>
        /// Creates an <see cref="RequestLogEntry"/> instance for HTTP requests.
        /// </summary>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="scheme">The HTTP scheme of the request.</param>
        /// <param name="host">The host that was requested.</param>
        /// <param name="uri">The URI of the request.</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="statusCode">The HTTP status code returned by the service.</param>
        /// <param name="startTime">The time the request was received.</param>
        /// <param name="duration">The duration of the processing of the request.</param>
        /// <param name="context">The context that provides more insights on the HTTP request that was tracked.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="statusCode"/> is outside the 0-999 range inclusively,
        ///     or the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static RequestLogEntry CreateForHttpRequest(
            string method,
            string scheme,
            string host,
            string uri,
            string operationName,
            int statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            IDictionary<string, object> context)
        {
            Guard.NotLessThan(statusCode, 0, nameof(statusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan(statusCode, 999, nameof(statusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            return new RequestLogEntry(
                method,
                $"{scheme}://{host}",
                uri,
                operationName ?? $"{method} {uri}",
                statusCode,
                RequestSourceSystem.Http,
                startTime.ToString(FormatSpecifiers.InvariantTimestampFormat),
                duration,
                context);
        }

        /// <summary>
        /// Creates an <see cref="RequestLogEntry"/> instance for Azure Service Bus requests.
        /// </summary>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static RequestLogEntry CreateForServiceBus(
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");
            
            return CreateWithoutHttpRequest(RequestSourceSystem.AzureServiceBus, operationName, isSuccessful, duration, startTime, context);
        }

        /// <summary>
        /// Creates an <see cref="RequestLogEntry"/> instance for Azure EventHubs requests.
        /// </summary>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure EventHubs request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure EventHubs request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static RequestLogEntry CreateForEventHubs(
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank operation name");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            return CreateWithoutHttpRequest(RequestSourceSystem.AzureEventHubs, operationName, isSuccessful, duration, startTime, context);
        }

        private static RequestLogEntry CreateWithoutHttpRequest(
            RequestSourceSystem source,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            return new RequestLogEntry(
                method: "<not-applicable>",
                host: "<not-applicable>",
                uri: "<not-applicable>",
                operationName: operationName,
                statusCode: isSuccessful ? 200 : 500,
                sourceSystem: source,
                requestTime: startTime.ToString(FormatSpecifiers.InvariantTimestampFormat),
                duration: duration,
                context: context);
        }

        /// <summary>
        /// Creates an <see cref="RequestLogEntry"/> instance for custom requests.
        /// </summary>
        /// <param name="requestSource">The source for the request telemetry to identifying the caller (ex. entity name of Azure Service Bus).</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="isSuccessful">The indication whether or not the custom request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the custom request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the custom request.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static RequestLogEntry CreateForCustomRequest(
            string requestSource,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            Guard.NotNullOrWhitespace(requestSource, nameof(requestSource), "Requires a non-blank request source to identify the caller");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank operation name");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request duration");

            return CreateWithoutHttpRequest(requestSource, operationName, isSuccessful, duration, startTime, context);
        }

        private static RequestLogEntry CreateWithoutHttpRequest(
            string requestSource,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            return new RequestLogEntry(
                method: "<not-applicable>",
                host: "<not-applicable>",
                uri: "<not-applicable>",
                operationName: operationName,
                statusCode: isSuccessful ? 200 : 500,
                customRequestSource: requestSource,
                requestTime: startTime.ToString(FormatSpecifiers.InvariantTimestampFormat),
                duration: duration,
                context: context);
        }

        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        public string RequestMethod { get; }
        
        /// <summary>
        /// Gets the host that was requested.
        /// </summary>
        public string RequestHost { get; }
        
        /// <summary>
        /// Gets ths URI of the request.
        /// </summary>
        public string RequestUri { get; }
        
        /// <summary>
        /// Gets the HTTP response status code that was returned by the service.
        /// </summary>
        public int ResponseStatusCode { get; }
        
        /// <summary>
        /// Gets the duration of the processing of the request.
        /// </summary>
        public TimeSpan RequestDuration { get; }
        
        /// <summary>
        /// Gets the date when the request occurred.
        /// </summary>
        public string RequestTime { get; }

        /// <summary>
        /// Gets the type of source system from where the request came from.
        /// </summary>
        public RequestSourceSystem SourceSystem { get; set; }

        /// <summary>
        /// Gets the custom request source if the <see cref="SourceSystem"/> is <see cref="RequestSourceSystem.Custom"/>.
        /// </summary>
        public string CustomRequestSource { get; }

        /// <summary>
        /// Gets the name of the operation of the source system from where the request came from.
        /// </summary>
        public string OperationName { get; }

        /// <summary>
        /// Gets the context that provides more insights on the HTTP request that was tracked.
        /// </summary>
        public IDictionary<string, object> Context { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var contextFormatted = $"{{{String.Join("; ", Context.Select(item => $"[{item.Key}, {item.Value}]"))}}}";
            
            if (SourceSystem is RequestSourceSystem.Http)
            {
                return $"{RequestMethod} {RequestHost}/{RequestUri} from {OperationName} completed with {ResponseStatusCode} in {RequestDuration} at {RequestTime} - (Context: {contextFormatted})";
            }

            string source = DetermineSource();
            bool isSuccessful = ResponseStatusCode is 200;
            
            return $"{source} from {OperationName} completed in {RequestDuration} at {RequestTime} - (IsSuccessful: {isSuccessful}, Context: {contextFormatted})";
        }

        private string DetermineSource()
        {
            switch (SourceSystem)
            {
                case RequestSourceSystem.AzureServiceBus: return "Azure Service Bus";
                case RequestSourceSystem.AzureEventHubs: return "Azure EventHubs";
                case RequestSourceSystem.Custom: return "Custom " + CustomRequestSource;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SourceSystem), "Cannot determine request source as it represents something outside the bounds of the enumeration");
            }
        }
    }
}
