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
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Name of the storage resource</param>
        /// <param name="database">Name of the database</param>
        /// <param name="container">Name of the container</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency of the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a Cosmos SQL dependency")]
        public static void LogCosmosSqlDependency(
            this ILogger logger,
            string accountName,
            string database,
            string container,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Cosmos SQL storage when tracking an Cosmos SQL dependency");

            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The name of the storage resource.</param>
        /// <param name="database">The name of the database.</param>
        /// <param name="container">The name of the container.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful</param>
        /// <param name="measurement">The measuring of the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        public static void LogCosmosSqlDependency(
            this ILogger logger,
            string accountName,
            string database,
            string container,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Cosmos SQL storage when tracking an Cosmos SQL dependency");

            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The name of the storage resource.</param>
        /// <param name="database">The name of the database.</param>
        /// <param name="container">The name of the container.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful</param>
        /// <param name="measurement">The measuring of the latency to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        public static void LogCosmosSqlDependency(
            this ILogger logger,
            string accountName,
            string database,
            string container,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Cosmos SQL storage when tracking an Cosmos SQL dependency");

            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The name of the storage resource.</param>
        /// <param name="database">The name of the database.</param>
        /// <param name="container">The name of the container.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogCosmosSqlDependency(
            this ILogger logger,
            string accountName,
            string database,
            string container,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Cosmos SQL operation");

            context = context ?? new Dictionary<string, object>();

            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The name of the storage resource.</param>
        /// <param name="database">The name of the database.</param>
        /// <param name="container">The name of the container.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogCosmosSqlDependency(
            this ILogger logger,
            string accountName,
            string database,
            string container,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Cosmos SQL operation");

            context = context ?? new Dictionary<string, object>();
            string data = $"{database}/{container}";

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure DocumentDB",
                dependencyName: data,
                dependencyData: data,
                targetName: accountName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful:
                isSuccessful,
                context: context));
        }
    }
}
