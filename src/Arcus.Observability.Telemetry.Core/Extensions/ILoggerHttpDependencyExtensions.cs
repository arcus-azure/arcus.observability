﻿using System;
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
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="measurement">The measuring the latency of the HTTP dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a HTTP dependency")]
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode),
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, context);
        }

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
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode),
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));

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
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode),
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));

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
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP dependency operation");
            Guard.For(() => request.RequestUri is null, new ArgumentException("Requires a HTTP request URI to track a HTTP dependency", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request method to track a HTTP dependency", nameof(request)));
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode),
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));

            context = context ?? new Dictionary<string, object>();

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
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP dependency operation");
            Guard.For(() => request.RequestUri is null, new ArgumentException("Requires a HTTP request URI to track a HTTP dependency", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request method to track a HTTP dependency", nameof(request)));
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode),
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));

            context = context ?? new Dictionary<string, object>();

            Uri requestUri = request.RequestUri;
            string targetName = requestUri.Host;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            bool isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;

            logger.LogWarning(MessageFormats.HttpDependencyFormat, new DependencyLogEntry(
                dependencyType: "Http",
                dependencyName: dependencyName,
                dependencyData: null,
                targetName: targetName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: (int) statusCode,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
