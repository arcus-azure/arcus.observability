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
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">The namespace of the resource.</param>
        /// <param name="eventHubName">The name of the Event Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        public static void LogEventHubsDependency(
            this ILogger logger,
            string namespaceName,
            string eventHubName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");
            }

            LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">The namespace of the resource.</param>
        /// <param name="eventHubName">The name of the Event Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        public static void LogEventHubsDependency(
            this ILogger logger,
            string namespaceName,
            string eventHubName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");
            }

            LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">The namespace of the resource.</param>
        /// <param name="eventHubName">The name of the Event Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogEventHubsDependency(
            this ILogger logger,
            string namespaceName,
            string eventHubName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
                => LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId: null, context);

        /// <summary>
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">The namespace of the resource.</param>
        /// <param name="eventHubName">The name of the Event Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogEventHubsDependency(
            this ILogger logger,
            string namespaceName,
            string eventHubName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger), "Requires a logger instance to track telemetry");
            }
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                throw new ArgumentNullException(nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            }
            if (string.IsNullOrWhiteSpace(eventHubName))
            {
                throw new ArgumentNullException(nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            }
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the Azure Events Hubs operation");
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure Event Hubs",
                dependencyName: eventHubName,
                dependencyData: namespaceName,
                targetName: eventHubName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
