using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Arcus.Observability.Telemetry.Core.Sql;
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
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, <paramref name="tableName"/>, or <paramref name="operationName"/> is blank.
        /// </exception>
        [Obsolete("Use the " + nameof(LogSqlDependency) + " with a pseudo SQL command and operation name instead of specifying the table name")]
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
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/> or <paramref name="databaseName"/> is blank.</exception>
        [Obsolete("Use the " + nameof(LogSqlDependency) + " with a pseudo SQL command and operation name instead of specifying the table name")]
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string sqlCommand,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, sqlCommand, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/> or <paramref name="databaseName"/> is blank.</exception>
        [Obsolete("Use the " + nameof(LogSqlDependency) + " with a pseudo SQL command and operation name instead of specifying the table name")]
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string sqlCommand,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, sqlCommand, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
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
        [Obsolete("Use the " + nameof(LogSqlDependency) + " with a pseudo SQL command and operation name instead of specifying the table name")]
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

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the SQL dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/> or <paramref name="databaseName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Use the " + nameof(LogSqlDependency) + " with a pseudo SQL command and operation name instead of specifying the table name")]
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string sqlCommand,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the SQL dependency operation");

            context = context ?? new Dictionary<string, object>();

            LogSqlDependency(logger, serverName, databaseName, sqlCommand, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the SQL dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/> or <paramref name="databaseName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Use the " + nameof(LogSqlDependency) + " with a pseudo SQL command and operation name instead of specifying the table name")]
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string sqlCommand,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the SQL dependency operation");

            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(MessageFormats.SqlDependencyFormat, new DependencyLogEntry(
                dependencyType: "Sql",
                targetName: serverName,
                dependencyName: databaseName,
                dependencyData: sqlCommand,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="operationName">The functional description of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/> or <paramref name="databaseName"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string sqlCommand,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, sqlCommand, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="operationName">The functional description of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the SQL dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/> or <paramref name="databaseName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string sqlCommand,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the SQL dependency operation");

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.SqlDependencyFormat, new DependencyLogEntry(
                dependencyType: "Sql",
                targetName: serverName,
                dependencyName: databaseName + "/" + operationName,
                dependencyData: sqlCommand,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="operationName">The functional description of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static void LogSqlDependencyWithConnectionString(
            this ILogger logger,
            string connectionString,
            string sqlCommand,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a SQL connection string to retrieve database information while tracking the SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependencyWithConnectionString(logger, connectionString, sqlCommand, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="operationName">The functional description of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">The duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static void LogSqlDependencyWithConnectionString(
            this ILogger logger,
            string connectionString,
            string sqlCommand,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a SQL connection string to retrieve database information while tracking the SQL dependency");

            var result = SqlConnectionStringParser.Parse(connectionString);
            
            string initialCatalog = result.InitialCatalog;
            if (string.IsNullOrEmpty(initialCatalog))
            {
                initialCatalog = "<not-available>";
            }

            logger.LogSqlDependency(result.DataSource, initialCatalog, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId, context);
        }
    }
}
