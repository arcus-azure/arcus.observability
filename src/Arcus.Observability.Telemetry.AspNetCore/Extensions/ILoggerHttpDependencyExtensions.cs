using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
    public static class ILoggerHttpDependencyExtensions
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
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequest request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.NotLessThan((int)statusCode, 100, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");
            Guard.NotGreaterThan((int)statusCode, 599, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");

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
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequest request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.NotLessThan((int)statusCode, 100, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");
            Guard.NotGreaterThan((int)statusCode, 599, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that started the HTTP communication</param>
        /// <param name="statusCode">Status code that was returned by the service for this HTTP communication</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequest request,
            HttpStatusCode statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP dependency operation");
            Guard.NotLessThan((int)statusCode, 100, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");
            Guard.NotGreaterThan((int)statusCode, 599, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");

            context = context ?? new Dictionary<string, object>();

            LogHttpDependency(logger, request, statusCode, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that started the HTTP communication</param>
        /// <param name="statusCode">Status code that was returned by the service for this HTTP communication</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequest request,
            HttpStatusCode statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP dependency operation");
            Guard.NotLessThan((int)statusCode, 100, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");
            Guard.NotGreaterThan((int)statusCode, 599, nameof(statusCode), "Requires a valid HTTP response status code that's within the range of 100 to 599, inclusive");

            context = context ?? new Dictionary<string, object>();

            string requestUri = request.Path;
            string targetName = request.Host.Host;
            string requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri}";
            bool isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;

            logger.LogWarning(MessageFormats.HttpDependencyFormat, new DependencyLogEntry(
                dependencyType: "Http",
                dependencyName: dependencyName,
                dependencyData: null,
                targetName: targetName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: (int)statusCode,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
