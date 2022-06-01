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
        /// Logs a SQL dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">Name of server hosting the database</param>
        /// <param name="databaseName">Name of database</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the SQL dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, or <paramref name="tableName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a SQL dependency")]
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string tableName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, tableName, measurement.DependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="tableName">The name of tracked table in the SQL database.</param>
        /// <param name="operationName">The name of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring of the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, <paramref name="tableName"/>, or <paramref name="operationName"/> is blank.
        /// </exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string tableName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name of the SQL operation to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, tableName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a SQL dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">Name of server hosting the database</param>
        /// <param name="databaseName">Name of database</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="operationName">Name of the operation that was performed</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, <paramref name="tableName"/>, or <paramref name="operationName"/> is blank.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string tableName,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name of the SQL operation to track a SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the SQL dependency operation");

            context = context ?? new Dictionary<string, object>();

            string dependencyName = $"{databaseName}/{tableName}";

            logger.LogWarning(MessageFormats.SqlDependencyFormat, new DependencyLogEntry(
                dependencyType: "Sql",
                targetName: serverName,
                dependencyName: dependencyName,
                dependencyData: operationName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
