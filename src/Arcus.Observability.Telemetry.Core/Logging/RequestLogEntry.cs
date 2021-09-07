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
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogEntry"/> class.
        /// </summary>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="host">The host that was requested.</param>
        /// <param name="uri">The URI of the request.</param>
        /// <param name="statusCode">The HTTP status code returned by the service.</param>
        /// <param name="duration">The duration of the processing of the request.</param>
        /// <param name="context">The context that provides more insights on the HTTP request that was tracked.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="duration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="uri"/>'s URI is blank,
        ///     the <paramref name="host"/> contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="statusCode"/>'s status code is outside the 100-599 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public RequestLogEntry(
            string method,
            string host,
            string uri,
            int statusCode,
            TimeSpan duration,
            IDictionary<string, object> context)
        {
            Guard.For<ArgumentException>(() => host?.Contains(" ") == true, "Requires a HTTP request host name without whitespace");
            Guard.NotLessThan(statusCode, 100, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotGreaterThan(statusCode, 599, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            RequestMethod = method;
            RequestHost = host;
            RequestUri = uri;
            ResponseStatusCode = statusCode;
            RequestDuration = duration;
            RequestTime = DateTimeOffset.UtcNow.ToString(FormatSpecifiers.InvariantTimestampFormat);
            Context = context;
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogEntry"/> class.
        /// </summary>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="host">The host that was requested.</param>
        /// <param name="uri">The URI of the request.</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="statusCode">The HTTP status code returned by the service.</param>
        /// <param name="duration">The duration of the processing of the request.</param>
        /// <param name="context">The context that provides more insights on the HTTP request that was tracked.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="duration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="uri"/>'s URI is blank,
        ///     the <paramref name="host"/> contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="statusCode"/>'s status code is outside the 100-599 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public RequestLogEntry(
            string method, 
            string host, 
            string uri, 
            string operationName,
            int statusCode,
            TimeSpan duration,
            IDictionary<string, object> context)
        {
            Guard.For<ArgumentException>(() => host?.Contains(" ") == true, "Requires a HTTP request host name without whitespace");
            Guard.NotLessThan(statusCode, 100, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotGreaterThan(statusCode, 599, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");
            
            RequestMethod = method;
            RequestHost = host;
            RequestUri = uri;
            ResponseStatusCode = statusCode;
            RequestDuration = duration;
            OperationName = operationName;
            RequestTime = DateTimeOffset.UtcNow.ToString(FormatSpecifiers.InvariantTimestampFormat);
            Context = context;
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Request;
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
        /// Gets the name of the operation.
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
            string contextFormatted = $"{{{String.Join("; ", Context.Select(item => $"[{item.Key}, {item.Value}]"))}}}";
            return $"{RequestMethod} {RequestHost}/{RequestUri} completed with {ResponseStatusCode} in {RequestDuration} at {RequestTime} - (Context: {contextFormatted})";
        }
    }
}
