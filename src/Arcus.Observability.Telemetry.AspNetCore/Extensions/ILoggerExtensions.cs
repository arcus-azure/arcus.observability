using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using GuardNet;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Telemetry extensions on the <see cref="ILogger"/> instance to write Application Insights compatible log messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s scheme contains whitespace, the <paramref name="request"/>'s host is missing or contains whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogRequest(this ILogger logger, HttpRequest request, HttpResponse response, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.For(() => !request.Path.HasValue, new ArgumentException("Requires a HTTP request with a path", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request with a HTTP method", nameof(request)));
            Guard.For(() => request.Scheme?.Contains(" ") == true, new ArgumentException("Requires a HTTP request scheme without whitespace to track a HTTP request", nameof(request)));
            Guard.For(() => !request.Host.HasValue, new ArgumentException("Requires a HTTP request with a host value to track a HTTP request", nameof(request)));
            Guard.For(() => request.Host.ToString()?.Contains(" ") == true, new ArgumentException("Requires a HTTP request host name without whitespace to track a HTTP request", nameof(request)));
            Guard.NotLessThan(response.StatusCode, 0, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan(response.StatusCode, 999, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Blob storage operation");

            LogRequest(logger, request, response.StatusCode, duration, context);
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s scheme contains whitespace, the <paramref name="request"/>'s host is missing or contains whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogRequest(this ILogger logger, HttpRequest request, HttpResponse response, string operationName, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.For(() => !request.Path.HasValue, new ArgumentException("Requires a HTTP request with a path", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request with a HTTP method", nameof(request)));
            Guard.For(() => request.Scheme?.Contains(" ") == true, new ArgumentException("Requires a HTTP request scheme without whitespace to track a HTTP request", nameof(request)));
            Guard.For(() => !request.Host.HasValue, new ArgumentException("Requires a HTTP request with a host value to track a HTTP request", nameof(request)));
            Guard.For(() => request.Host.ToString()?.Contains(" ") == true, new ArgumentException("Requires a HTTP request host name without whitespace to track a HTTP request", nameof(request)));
            Guard.NotLessThan(response.StatusCode, 0, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan(response.StatusCode, 999, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Blob storage operation");

            LogRequest(logger, request, response.StatusCode, operationName, duration, context);
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s scheme contains whitespace, the <paramref name="request"/>'s host is missing or contains whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogRequest(this ILogger logger, HttpRequest request, int responseStatusCode, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.For(() => !request.Path.HasValue, new ArgumentException("Requires a HTTP request with a path", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request with a HTTP method", nameof(request)));
            Guard.For(() => request.Scheme?.Contains(" ") == true, new ArgumentException("Requires a HTTP request scheme without whitespace to track a HTTP request", nameof(request)));
            Guard.For(() => !request.Host.HasValue, new ArgumentException("Requires a HTTP request with a host value to track a HTTP request", nameof(request)));
            Guard.For(() => request.Host.ToString()?.Contains(" ") == true, new ArgumentException("Requires a HTTP request host name without whitespace to track a HTTP request", nameof(request)));
            Guard.NotLessThan(responseStatusCode, 0, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan(responseStatusCode, 999, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP request");
            
            context = context ?? new Dictionary<string, object>();

            PathString resourcePath = request.Path;
            var host = $"{request.Scheme}://{request.Host}";

            logger.LogWarning(MessageFormats.RequestFormat, new RequestLogEntry(request.Method, host, resourcePath, responseStatusCode, duration, context));
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s scheme contains whitespace, the <paramref name="request"/>'s host is missing or contains whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogRequest(this ILogger logger, HttpRequest request, int responseStatusCode, string operationName, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.For(() => !request.Path.HasValue, new ArgumentException("Requires a HTTP request with a path", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request with a HTTP method", nameof(request)));
            Guard.For(() => request.Scheme?.Contains(" ") == true, new ArgumentException("Requires a HTTP request scheme without whitespace to track a HTTP request", nameof(request)));
            Guard.For(() => !request.Host.HasValue, new ArgumentException("Requires a HTTP request with a host value to track a HTTP request", nameof(request)));
            Guard.For(() => request.Host.ToString()?.Contains(" ") == true, new ArgumentException("Requires a HTTP request host name without whitespace to track a HTTP request", nameof(request)));
            Guard.NotLessThan(responseStatusCode, 0, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan(responseStatusCode, 999, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP request");

            context = context ?? new Dictionary<string, object>();

            PathString resourcePath = request.Path;
            var host = $"{request.Scheme}://{request.Host}";

            logger.LogWarning(MessageFormats.RequestFormat, new RequestLogEntry(request.Method, host, resourcePath, operationName, responseStatusCode, duration, context));
        }
    }
}
