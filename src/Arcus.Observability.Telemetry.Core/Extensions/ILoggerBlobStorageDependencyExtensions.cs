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
        /// Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="containerName">Name of the Blob Container resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="containerName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Blob storage dependency")]
        public static void LogBlobStorageDependency(
            this ILogger logger,
            string accountName,
            string containerName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Blob storage resource to track an Azure Blob storage dependency");
            Guard.NotNullOrWhitespace(containerName, nameof(containerName), "Requires a non-blank container name in the Azure BLob storage resource to track an Azure Blob storage dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Blob storage when tracking an Azure Blob storage dependency");

            LogBlobStorageDependency(logger, accountName, containerName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The account of the storage resource.</param>
        /// <param name="containerName">The name of the Blob Container resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="containerName"/> is blank.</exception>
        public static void LogBlobStorageDependency(
            this ILogger logger,
            string accountName,
            string containerName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Blob storage resource to track an Azure Blob storage dependency");
            Guard.NotNullOrWhitespace(containerName, nameof(containerName), "Requires a non-blank container name in the Azure BLob storage resource to track an Azure Blob storage dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Blob storage when tracking an Azure Blob storage dependency");

            LogBlobStorageDependency(logger, accountName, containerName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The account of the storage resource.</param>
        /// <param name="containerName">The name of the Blob Container resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="containerName"/> is blank.</exception>
        public static void LogBlobStorageDependency(
            this ILogger logger,
            string accountName,
            string containerName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Blob storage resource to track an Azure Blob storage dependency");
            Guard.NotNullOrWhitespace(containerName, nameof(containerName), "Requires a non-blank container name in the Azure BLob storage resource to track an Azure Blob storage dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Blob storage when tracking an Azure Blob storage dependency");

            LogBlobStorageDependency(logger, accountName, containerName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="containerName">Name of the Blob Container resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>nul</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="containerName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogBlobStorageDependency(
            this ILogger logger,
            string accountName,
            string containerName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Blob storage resource to track an Azure Blob storage dependency");
            Guard.NotNullOrWhitespace(containerName, nameof(containerName), "Requires a non-blank container name in the Azure BLob storage resource to track an Azure Blob storage dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Blob storage operation");

            context = context ?? new Dictionary<string, object>();

            LogBlobStorageDependency(logger, accountName, containerName, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="containerName">Name of the Blob Container resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>nul</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="containerName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogBlobStorageDependency(
            this ILogger logger,
            string accountName,
            string containerName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Blob storage resource to track an Azure Blob storage dependency");
            Guard.NotNullOrWhitespace(containerName, nameof(containerName), "Requires a non-blank container name in the Azure BLob storage resource to track an Azure Blob storage dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Blob storage operation");

            context = context ?? new Dictionary<string, object>();

            string dependencyName = $"{accountName}/{containerName}";

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure blob",
                dependencyName: dependencyName,
                dependencyData: containerName,
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
