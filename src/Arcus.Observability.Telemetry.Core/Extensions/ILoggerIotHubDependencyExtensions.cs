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
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">Name of the IoT Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure IoT Hub dependency")]
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");

            LogIotHubDependency(logger, iotHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">The nme of the IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");

            LogIotHubDependency(logger, iotHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">The nme of the IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");

            LogIotHubDependency(logger, iotHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">The name of the IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the IoT Hub operation");

            context = context ?? new Dictionary<string, object>();

            LogIotHubDependency(logger, iotHubName, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">The name of the IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the IoT Hub operation");

            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure IoT Hub",
                dependencyName: iotHubName,
                dependencyData: null,
                targetName: iotHubName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
