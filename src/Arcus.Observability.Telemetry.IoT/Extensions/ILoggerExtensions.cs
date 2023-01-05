using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Microsoft.Azure.Devices.Client;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions on the <see cref="ILogger"/> related to tracking IoT dependencies.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        /// <summary>
        ///     Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="iotHubConnectionString">Name of the IoT Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track IoT Hub dependencies")]
        public static void LogIotHubDependency(this ILogger logger, string iotHubConnectionString, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString));

            LogIotHubDependency(logger, iotHubConnectionString, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger instance to track the IoT Hub dependency.</param>
        /// <param name="iotHubConnectionString">The connection string to interact with an IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubConnectionString"/> is blank or is invalid.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="iotHubConnectionString"/> is invalid.</exception>
        [Obsolete("Use the 'LogIotHubDependencyWithConnectionString' instead in the 'Arcus.Observability.Telemetry.Core' package and remove the 'Arcus.Observability.Telemetry.IoT' package from your project")]
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubConnectionString,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track the IoT Hub dependency");
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString), "Requires an IoT Hub connection string to retrieve the IoT host name to track the IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires an measurement instance to measure the duration of interaction with the IoT Hub dependency");

            context = context ?? new Dictionary<string, object>();

            LogIotHubDependency(logger, iotHubConnectionString, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger instance to track the IoT Hub dependency.</param>
        /// <param name="iotHubConnectionString">The connection string to interact with an IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubConnectionString"/> is blank or is invalid.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="iotHubConnectionString"/> is invalid.</exception>
        [Obsolete("Use the 'LogIotHubDependencyWithConnectionString' instead in the 'Arcus.Observability.Telemetry.Core' package and remove the 'Arcus.Observability.Telemetry.IoT' package from your project")]
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubConnectionString,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track the IoT Hub dependency");
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString), "Requires an IoT Hub connection string to retrieve the IoT host name to track the IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires an measurement instance to measure the duration of interaction with the IoT Hub dependency");

            context = context ?? new Dictionary<string, object>();

            LogIotHubDependency(logger, iotHubConnectionString, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger instance to track the IoT Hub dependency.</param>
        /// <param name="iotHubConnectionString">The connection string to interact with an IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubConnectionString"/> is blank or is invalid.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="iotHubConnectionString"/> is invalid.</exception>
        [Obsolete("Use the 'LogIotHubDependencyWithConnectionString' instead in the 'Arcus.Observability.Telemetry.Core' package and remove the 'Arcus.Observability.Telemetry.IoT' package from your project")]
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubConnectionString,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track the IoT Hub dependency");
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString), "Requires an IoT Hub connection string to retrieve the IoT host name to track the IoT Hub dependency");

            context = context ?? new Dictionary<string, object>();

            LogIotHubDependency(logger, iotHubConnectionString, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger instance to track the IoT Hub dependency.</param>
        /// <param name="iotHubConnectionString">The connection string to interact with an IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubConnectionString"/> is blank or is invalid.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="iotHubConnectionString"/> is invalid.</exception>
        [Obsolete("Use the 'LogIotHubDependencyWithConnectionString' instead in the 'Arcus.Observability.Telemetry.Core' package and remove the 'Arcus.Observability.Telemetry.IoT' package from your project")]
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubConnectionString,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track the IoT Hub dependency");
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString), "Requires an IoT Hub connection string to retrieve the IoT host name to track the IoT Hub dependency");

            context = context ?? new Dictionary<string, object>();

            var iotHubConnection = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            logger.LogIotHubDependency(iotHubName: iotHubConnection.HostName, isSuccessful, startTime, duration, dependencyId, context);
        }
    }
}
