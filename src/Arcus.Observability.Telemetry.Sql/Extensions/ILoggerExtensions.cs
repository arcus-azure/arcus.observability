using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Arcus.Observability.Telemetry.Core;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions on the <see cref="ILogger"/> related to tracking SQL dependencies.
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
        public static void LogSqlDependency(this ILogger logger, string connectionString, string tableName, string operationName, DependencyMeasurement measurement, bool isSuccessful, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, connectionString, tableName, operationName, measurement.StartTime, measurement.Elapsed, isSuccessful, context);
        }

        /// <summary>
        ///     Logs a SQL dependency
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="connectionString">SQL connection string</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="operationName">Name of the operation that was performed</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogSqlDependency(this ILogger logger, string connectionString, string tableName, string operationName, DateTimeOffset startTime, TimeSpan duration, bool isSuccessful, Dictionary<string, object> context = null)
        {
            Guard.NotNullOrEmpty(connectionString, nameof(connectionString));
            var connection = new SqlConnectionStringBuilder(connectionString);

            logger.LogSqlDependency(connection.DataSource, connection.InitialCatalog, tableName, operationName, isSuccessful, startTime, duration, context);
        }
    }
}
