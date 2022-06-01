using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Arcus.Observability.Telemetry.Core;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions related to the tracking of SQL dependencies.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        /// <summary>
        ///     Logs a SQL dependency
        /// </summary>	
        /// <param name="logger">Logger to use</param>
        /// <param name="connectionString">SQL connection string</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="operationName">Name of the operation that was performed</param>	
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the SQL dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a SQL dependency")]
        public static void LogSqlDependency(
            this ILogger logger, 
            string connectionString, 
            string tableName, 
            string operationName, 
            DependencyMeasurement measurement, 
            bool isSuccessful, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, connectionString, tableName, operationName, measurement.StartTime, measurement.Elapsed, isSuccessful, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>	
        /// <param name="logger">The logger instance to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="tableName">The name of tracked table in the SQL database.</param>
        /// <param name="operationName">The name of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="tableName"/> or <paramref name="operationName"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string connectionString,
            string tableName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name of the SQL operation to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, connectionString, tableName, operationName, measurement.StartTime, measurement.Elapsed, isSuccessful, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>	
        /// <param name="logger">The logger instance to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string connectionString,
            string sqlCommand,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a SQL connection string to retrieve database information while tracking the SQL dependency");
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, connectionString, sqlCommand, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger instance to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string connectionString,
            string sqlCommand,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a SQL connection string to retrieve database information while tracking the SQL dependency");
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, connectionString, sqlCommand, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="tableName">The name of tracked table in the SQL database.</param>	
        /// <param name="operationName">The name of the operation that was performed on the SQL database.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">The duration of the operation</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="tableName"/> or <paramref name="operationName"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string connectionString,
            string tableName,
            string operationName,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool isSuccessful,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrEmpty(connectionString, nameof(connectionString));
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name of the SQL operation to track a SQL dependency");

            var connection = new SqlConnectionStringBuilder(connectionString);
            logger.LogSqlDependency(connection.DataSource, connection.InitialCatalog, tableName, operationName, isSuccessful, startTime, duration, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">The duration of the operation</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string connectionString,
            string sqlCommand,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a SQL connection string to retrieve database information while tracking the SQL dependency");

            var connection = new SqlConnectionStringBuilder(connectionString);
            logger.LogSqlDependency(connection.DataSource, connection.InitialCatalog, sqlCommand, isSuccessful, startTime, duration, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the SQL dependency.</param>
        /// <param name="connectionString">The SQL connection string.</param>
        /// <param name="sqlCommand">The pseudo SQL command information that gets executed against the SQL dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">The duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string connectionString,
            string sqlCommand,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a SQL connection string to retrieve database information while tracking the SQL dependency");

            var connection = new SqlConnectionStringBuilder(connectionString);
            logger.LogSqlDependency(connection.DataSource, connection.InitialCatalog, sqlCommand, isSuccessful, startTime, duration, dependencyId, context);
        }
    }
}
