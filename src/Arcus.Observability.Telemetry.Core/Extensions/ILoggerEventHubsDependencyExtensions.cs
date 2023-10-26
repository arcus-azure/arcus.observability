using System;
using System.Collections.Generic;
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
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">Namespace of the resource</param>
        /// <param name="eventHubName">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Event Hubs dependency")]
        public static void LogEventHubsDependency(
            this ILogger logger,
            string namespaceName,
            string eventHubName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Event Hub resource when tracking an Azure Event Hub dependency");

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
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Event Hub resource when tracking an Azure Event Hub dependency");

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
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Event Hub resource when tracking an Azure Event Hub dependency");

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
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Events Hubs operation");

            LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId: null, context);
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
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Events Hubs operation");

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
