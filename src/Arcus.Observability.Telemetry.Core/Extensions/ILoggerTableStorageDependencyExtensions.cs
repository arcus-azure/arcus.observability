﻿using System;
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
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The account of the storage resource.</param>
        /// <param name="tableName">The name of the Table Storage resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="tableName"/> is blank.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogTableStorageDependency(
            this ILogger logger,
            string accountName,
            string tableName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogTableStorageDependency(logger, accountName, tableName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The account of the storage resource.</param>
        /// <param name="tableName">The name of the Table Storage resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="tableName"/> is blank.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogTableStorageDependency(
            this ILogger logger,
            string accountName,
            string tableName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogTableStorageDependency(logger, accountName, tableName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="tableName">Name of the Table Storage resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>nul</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="tableName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogTableStorageDependency(
            this ILogger logger,
            string accountName,
            string tableName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogTableStorageDependency(logger, accountName, tableName, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="tableName">Name of the Table Storage resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>nul</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="tableName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogTableStorageDependency(
            this ILogger logger,
            string accountName,
            string tableName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentException("Requires a non-blank account name for the Azure Table storage resource to track an Azure Table storage dependency", nameof(accountName));
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Requires a non-blank table name in the Azure Table storage resource to track an Azure Table storage dependency", nameof(tableName));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the Azure Table storage operation");
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            string dependencyName = $"{accountName}/{tableName}";

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure table",
                dependencyName: dependencyName,
                dependencyData: tableName,
                targetName: accountName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
