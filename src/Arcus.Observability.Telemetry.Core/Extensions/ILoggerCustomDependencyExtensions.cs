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
    [Obsolete("Will be removed in v4.0 in favor of a Serilog-specific implementation")]
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency.</param>
        /// <param name="dependencyData">Custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful, startTime, duration, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful, startTime, duration, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful: isSuccessful, dependencyName, startTime: startTime, duration: duration, context: context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful, dependencyName, startTime, duration, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of the dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of the dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of the dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of the dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(measurement);
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName: targetName, startTime, duration, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, targetName, startTime, duration, dependencyId, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(dependencyType);
            ArgumentNullException.ThrowIfNull(dependencyData);
            ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType,
                dependencyName: dependencyName,
                dependencyData: dependencyData,
                targetName: targetName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
