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
        [Obsolete("Will be removed in v4.0 as the Microsoft SQL client supports telemetry now natively")]
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
        [Obsolete("Will be removed in v4.0 as the Microsoft SQL client supports telemetry now natively")]
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
        [Obsolete("Will be removed in v4.0 as the Microsoft SQL client supports telemetry now natively")]
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
        [Obsolete("Will be removed in v4.0 as the Microsoft SQL client supports telemetry now natively")]
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
