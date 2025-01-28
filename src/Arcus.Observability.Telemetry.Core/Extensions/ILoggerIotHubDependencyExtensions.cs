using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Iot;
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
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");
            }

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
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");
            }

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
                => LogIotHubDependency(logger, iotHubName, isSuccessful, startTime, duration, dependencyId: null, context);

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
        public static void LogIotHubDependencyWithConnectionString(
            this ILogger logger,
            string iotHubConnectionString,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (string.IsNullOrWhiteSpace(iotHubConnectionString))
            {
                throw new ArgumentNullException(nameof(iotHubConnectionString), "Requires an IoT Hub connection string to retrieve the IoT host name to track the IoT Hub dependency");
            }
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a measurement instance to measure the duration of interaction with the IoT Hub dependency");
            }

            LogIotHubDependencyWithConnectionString(logger, iotHubConnectionString, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
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
        public static void LogIotHubDependencyWithConnectionString(
            this ILogger logger,
            string iotHubConnectionString,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (string.IsNullOrWhiteSpace(iotHubConnectionString))
            {
                throw new ArgumentNullException(nameof(iotHubConnectionString), "Requires an IoT Hub connection string to retrieve the IoT host name to track the IoT Hub dependency");
            }

            var result = IotHubConnectionStringParser.Parse(iotHubConnectionString);
            LogIotHubDependency(logger, iotHubName: result.HostName, isSuccessful, startTime, duration, dependencyId, context);
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
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger), "Requires a logger instance to track telemetry");
            }
            if (string.IsNullOrWhiteSpace(iotHubName))
            {
                throw new ArgumentNullException(nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            }
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the IoT Hub operation");
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

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
