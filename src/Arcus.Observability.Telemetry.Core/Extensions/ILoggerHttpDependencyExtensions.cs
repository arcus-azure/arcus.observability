using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;

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
        /// Logs an HTTP dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside 100-599 range inclusively.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Microsoft HTTP client supports telemetry now natively")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside 100-599 range inclusively.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Microsoft HTTP client supports telemetry now natively")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an HTTP dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside 100-599 range inclusively.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Microsoft HTTP client supports telemetry now natively")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            int statusCode,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside 100-599 range inclusively.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Microsoft HTTP client supports telemetry now natively")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogHttpDependency(logger, request, statusCode, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside 100-599 range inclusively.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Microsoft HTTP client supports telemetry now natively")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            LogHttpDependency(logger, request, (int)statusCode, startTime, duration, dependencyId, context);
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside 100-599 range inclusively.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Microsoft HTTP client supports telemetry now natively")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            int statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the HTTP dependency operation");
            }

            if (request.RequestUri is null)
            {
                throw new ArgumentException("Requires a HTTP request URI to track a HTTP dependency", nameof(request));
            }

            if (request.Method is null)
            {
                throw new ArgumentException("Requires a HTTP request method to track a HTTP dependency", nameof(request));
            }

            if (statusCode < 100 || statusCode > 599)
            {
                throw new ArgumentOutOfRangeException(nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            Uri requestUri = request.RequestUri;
            string targetName = requestUri.Host;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            bool isSuccessful = statusCode >= 200 && statusCode < 300;

            logger.LogWarning(MessageFormats.HttpDependencyFormat, new DependencyLogEntry(
                dependencyType: "Http",
                dependencyName: dependencyName,
                dependencyData: null,
                targetName: targetName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: statusCode,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
