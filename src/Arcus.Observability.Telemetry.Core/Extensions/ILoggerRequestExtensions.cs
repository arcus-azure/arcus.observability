using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Telemetry extensions on the <see cref="ILogger"/> instance to write Application Insights compatible log messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="response">The outgoing HTTP response that was created.</param>
        /// <param name="measurement">The instance to measure the duration of the HTTP request.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/>, or the <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="response"/>'s status code is outside the 0-999 inclusively.</exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpResponseMessage response,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an measurement instance to time the duration of the HTTP request");

            LogRequest(logger, request, response, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="response">The outgoing HTTP response that was created.</param>
        /// <param name="startTime">The time when the HTTP request was received.</param>
        /// <param name="duration">The duration of the HTTP request processing operation.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="response"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpResponseMessage response,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            LogRequest(logger, request, response.StatusCode, startTime, duration, context);
        }

        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="response">The outgoing HTTP response that was created.</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="measurement">The instance to measure the duration of the HTTP request.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/>, or the <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="response"/>'s status code is outside the 0-999 inclusively.</exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string operationName,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an measurement instance to time the duration of the HTTP request");

            LogRequest(logger, request, response, operationName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="response">The outgoing HTTP response that was created.</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="startTime">The time when the HTTP request was received.</param>
        /// <param name="duration">The duration of the HTTP request processing operation.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="response"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string operationName,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            LogRequest(logger, request, response.StatusCode, operationName, startTime, duration, context);
        }

        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="responseStatusCode">The HTTP status code returned by the service.</param>
        /// <param name="measurement">The instance to measure the duration of the HTTP request.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or the <paramref name="measurement"/>  is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="responseStatusCode"/>'s status code is outside the 0-999 inclusively.</exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode responseStatusCode,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an measurement instance to time the duration of the HTTP request");

            LogRequest(logger, request, responseStatusCode, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="responseStatusCode">The HTTP status code returned by the service.</param>
        /// <param name="startTime">The time when the HTTP request was received.</param>
        /// <param name="duration">The duration of the HTTP request processing operation.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode responseStatusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            LogRequest(logger, request, responseStatusCode, operationName: null, startTime, duration, context);
        }

        /// <summary>
        /// Logs an HTTP request.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="responseStatusCode">The HTTP status code returned by the service.</param>
        /// <param name="operationName">The name of the operation of the HTTP request.</param>
        /// <param name="measurement">The instance to measure the duration of the HTTP request.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="responseStatusCode"/>'s status code is outside the 0-999 inclusively.</exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode responseStatusCode,
            string operationName,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an measurement instance to time the duration of the HTTP request");

            LogRequest(logger, request, responseStatusCode, operationName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP request
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The incoming HTTP request that was processed.</param>
        /// <param name="responseStatusCode">The HTTP status code returned by the service.</param>
        /// <param name="operationName">The name of the operation of the HTTP request.</param>
        /// <param name="startTime">The time when the HTTP request was received.</param>
        /// <param name="duration">The duration of the HTTP request processing operation.</param>
        /// <param name="context">The context that provides more insights on the tracked HTTP request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="responseStatusCode"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode responseStatusCode,
            string operationName,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(request.RequestUri, nameof(request), "Requires a request URI to retrieve the necessary request information");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.RequestFormat,
                RequestLogEntry.CreateForHttpRequest(
                    request.Method.ToString(),
                    request.RequestUri.Scheme,
                    request.RequestUri.Host,
                    request.RequestUri.AbsolutePath,
                    operationName,
                    (int)responseStatusCode,
                    startTime,
                    duration,
                    context));
        }
    }
}
