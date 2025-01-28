using System;
using System.Collections.Generic;
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
        /// Logs a custom request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="requestSource">The source for the request telemetry to identifying the caller (ex. entity name of Azure Service Bus).</param>
        /// <param name="isSuccessful">The indication whether or not the custom request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the custom request.</param>
        /// <param name="context">The telemetry context that provides more insights on the custom request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="requestSource"/> is blank.</exception>
        public static void LogCustomRequest(
            this ILogger logger,
            string requestSource,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires an instance to measure the custom request process latency duration");
            }

            LogCustomRequest(logger, requestSource, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a custom request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="requestSource">The source for the request telemetry to identifying the caller (ex. entity name of Azure Service Bus).</param>
        /// <param name="isSuccessful">The indication whether or not the custom request was successfully processed.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="duration">The duration it took to process the custom request.</param>
        /// <param name="context">The telemetry context that provides more insights on the custom request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="requestSource"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogCustomRequest(
            this ILogger logger,
            string requestSource,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
                => LogCustomRequest(logger, requestSource, operationName: null, isSuccessful, startTime, duration, context);

        /// <summary>
        /// Logs a custom request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="requestSource">The source for the request telemetry to identifying the caller (ex. entity name of Azure Service Bus).</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the custom request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the custom request.</param>
        /// <param name="context">The telemetry context that provides more insights on the custom request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="requestSource"/> is blank.</exception>
        public static void LogCustomRequest(
            this ILogger logger,
            string requestSource,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires an instance to measure the custom request process latency duration");
            }

            LogCustomRequest(logger, requestSource, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a custom request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="requestSource">The source for the request telemetry to identifying the caller (ex. entity name of Azure Service Bus).</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the custom request was successfully processed.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="duration">The duration it took to process the custom request.</param>
        /// <param name="context">The telemetry context that provides more insights on the custom request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="requestSource"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogCustomRequest(
            this ILogger logger, 
            string requestSource,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger), "Requires a logger instance to track telemetry");
            }
            if (string.IsNullOrWhiteSpace(requestSource))
            {
                throw new ArgumentNullException(nameof(requestSource), "Requires a non-blank request source to identify the caller");
            }
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the custom request operation");
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                operationName = ContextProperties.RequestTracking.DefaultOperationName;
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.RequestFormat, RequestLogEntry.CreateForCustomRequest(requestSource, operationName, isSuccessful, duration, startTime, context));
        }
    }
}
