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
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serviceBusNamespaceEndpoint">The namespace where the Azure Service Bus entity is located.</param>
        /// <param name="queueName">The name of the Service Bus queue.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="queueName"/> is blank.</exception>
        public static void LogServiceBusQueueDependency(
            this ILogger logger,
            string serviceBusNamespaceEndpoint,
            string queueName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus when tracking the Azure Service Bus dependency");
            }

            LogServiceBusQueueDependency(logger, serviceBusNamespaceEndpoint, queueName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serviceBusNamespaceEndpoint">The namespace where the Azure Service Bus entity is located.</param>
        /// <param name="queueName">Name of the Service Bus queue</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="queueName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusQueueDependency(
            this ILogger logger,
            string serviceBusNamespaceEndpoint,
            string queueName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            LogServiceBusDependency(logger, serviceBusNamespaceEndpoint, queueName, isSuccessful, startTime, duration, dependencyId, ServiceBusEntityType.Queue, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serviceBusNamespaceEndpoint">The namespace where the Azure Service Bus entity is located.</param>
        /// <param name="topicName">The name of the Service Bus topic.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="topicName"/> is blank.</exception>
        public static void LogServiceBusTopicDependency(
            this ILogger logger,
            string serviceBusNamespaceEndpoint,
            string topicName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus when tracking the Azure Service Bus dependency");
            }

            LogServiceBusTopicDependency(logger, serviceBusNamespaceEndpoint, topicName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serviceBusNamespaceEndpoint">The namespace where the Azure Service Bus entity is located.</param>
        /// <param name="topicName">Name of the Service Bus topic</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="topicName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusTopicDependency(
            this ILogger logger,
            string serviceBusNamespaceEndpoint,
            string topicName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            LogServiceBusDependency(logger, serviceBusNamespaceEndpoint, topicName, isSuccessful, startTime, duration, dependencyId, ServiceBusEntityType.Topic, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serviceBusNamespaceEndpoint">The namespace where the Azure Service Bus entity is located.</param>
        /// <param name="entityName">The name of the Service Bus entity.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="entityType">The type of the Service Bus entity.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> is blank.</exception>
        public static void LogServiceBusDependency(
            this ILogger logger,
            string serviceBusNamespaceEndpoint,
            string entityName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus when tracking the Azure Service Bus dependency");
            }

            LogServiceBusDependency(logger, serviceBusNamespaceEndpoint, entityName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serviceBusNamespaceEndpoint">The namespace where the Azure Service Bus entity is located.</param>
        /// <param name="entityName">The name of the Azure Service Bus entity.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="entityType">Type of the Service Bus entity.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> or <paramref name="serviceBusNamespaceEndpoint"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusDependency(
            this ILogger logger,
            string serviceBusNamespaceEndpoint,
            string entityName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger), "Requires a logger instance to track telemetry");
            }

            if (string.IsNullOrWhiteSpace(serviceBusNamespaceEndpoint))
            {
                throw new ArgumentException("Requires a non-blank namespace where the Azure Service Bus entity is located to track an Azure Service Bus dependency", nameof(serviceBusNamespaceEndpoint));
            }

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentException("Requires a non-blank Azure Service Bus entity name to track an Azure Service Bus dependency", nameof(entityName));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the Azure Service Bus operation");
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);
            context[ContextProperties.DependencyTracking.ServiceBus.EntityType] = entityType;
            context[ContextProperties.DependencyTracking.ServiceBus.Endpoint] = serviceBusNamespaceEndpoint;

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure Service Bus",
                dependencyName: entityName,
                dependencyData: null,
                dependencyId: dependencyId,
                targetName: entityName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
