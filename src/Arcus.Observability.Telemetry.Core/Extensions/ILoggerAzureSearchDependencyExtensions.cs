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
        /// Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="searchServiceName">The name of the Azure Search service.</param>
        /// <param name="operationName">The name of the operation to execute on the Azure Search service.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="searchServiceName"/> or <paramref name="operationName"/> is blank.</exception>
        public static void LogAzureSearchDependency(
            this ILogger logger,
            string searchServiceName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track the Azure Search dependency");
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Search resource when tracking the Azure Search dependency");

            LogAzureSearchDependency(logger, searchServiceName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="searchServiceName">The name of the Azure Search service.</param>
        /// <param name="operationName">The name of the operation to execute on the Azure Search service.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="searchServiceName"/> or <paramref name="operationName"/> is blank.</exception>
        public static void LogAzureSearchDependency(
            this ILogger logger,
            string searchServiceName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track the Azure Search dependency");
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Search resource when tracking the Azure Search dependency");

            LogAzureSearchDependency(logger, searchServiceName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="searchServiceName">Name of the Azure Search service</param>
        /// <param name="operationName">Name of the operation to execute on the Azure Search service</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="searchServiceName"/> or <paramref name="operationName"/> is blank.</exception>
        public static void LogAzureSearchDependency(
            this ILogger logger,
            string searchServiceName,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Search operation");

            LogAzureSearchDependency(logger, searchServiceName, operationName, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="searchServiceName">Name of the Azure Search service</param>
        /// <param name="operationName">Name of the operation to execute on the Azure Search service</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="searchServiceName"/> or <paramref name="operationName"/> is blank.</exception>
        public static void LogAzureSearchDependency(
            this ILogger logger,
            string searchServiceName,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Search operation");

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure Search",
                dependencyName: searchServiceName,
                dependencyData: operationName,
                dependencyId: dependencyId,
                targetName: searchServiceName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
