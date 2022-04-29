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
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="queueName">Name of the Service Bus queue</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Service Bus queue dependency")]
        public static void LogServiceBusQueueDependency(
            this ILogger logger,
            string queueName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus Queue when tracking the Azure Service Bus Queue dependency");

            LogServiceBusQueueDependency(logger, queueName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="queueName">The name of the Service Bus queue.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Service Bus dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="queueName"/> is blank.</exception>
        public static void LogServiceBusQueueDependency(
            this ILogger logger,
            string queueName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires a non-blank Azure Service Bus Queue name to track an Azure Service Bus Queue dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus Queue when tracking the Azure Service Bus Queue dependency");

            LogServiceBusQueueDependency(logger, queueName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="queueName">The name of the Service Bus queue.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Service Bus dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="queueName"/> is blank.</exception>
        public static void LogServiceBusQueueDependency(
            this ILogger logger,
            string queueName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires a non-blank Azure Service Bus Queue name to track an Azure Service Bus Queue dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus Queue when tracking the Azure Service Bus Queue dependency");

            LogServiceBusQueueDependency(logger, queueName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="queueName">Name of the Service Bus queue</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="queueName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusQueueDependency(
            this ILogger logger,
            string queueName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires a non-blank Azure Service Bus Queue name to track an Azure Service Bus Queue dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus Queue operation");

            LogServiceBusDependency(logger, queueName, isSuccessful, startTime, duration, ServiceBusEntityType.Queue, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
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
            string queueName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires a non-blank Azure Service Bus Queue name to track an Azure Service Bus Queue dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus Queue operation");

            LogServiceBusDependency(logger, queueName, isSuccessful, startTime, duration, dependencyId, ServiceBusEntityType.Queue, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="topicName">Name of the Service Bus topic</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="topicName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Service Bus topic dependency")]
        public static void LogServiceBusTopicDependency(
            this ILogger logger,
            string topicName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires a non-blank Azure Service Bus Topic name to track an Azure Service Bus Topic dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus Topic when tracking the Azure Service Bus Topic dependency");

            LogServiceBusTopicDependency(logger, topicName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="topicName">The name of the Service Bus topic.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Service Bus dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="topicName"/> is blank.</exception>
        public static void LogServiceBusTopicDependency(
            this ILogger logger,
            string topicName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires a non-blank Azure Service Bus Topic name to track an Azure Service Bus Topic dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus Topic when tracking the Azure Service Bus Topic dependency");

            LogServiceBusTopicDependency(logger, topicName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="topicName">The name of the Service Bus topic.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Service Bus dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="topicName"/> is blank.</exception>
        public static void LogServiceBusTopicDependency(
            this ILogger logger,
            string topicName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires a non-blank Azure Service Bus Topic name to track an Azure Service Bus Topic dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus Topic when tracking the Azure Service Bus Topic dependency");

            LogServiceBusTopicDependency(logger, topicName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="topicName">Name of the Service Bus topic</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="topicName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusTopicDependency(
            this ILogger logger,
            string topicName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires a non-blank Azure Service Bus Topic name to track an Azure Service Bus Topic dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus Topic operation");

            LogServiceBusDependency(logger, topicName, isSuccessful, startTime, duration, ServiceBusEntityType.Topic, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
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
            string topicName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires a non-blank Azure Service Bus Topic name to track an Azure Service Bus Topic dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus Topic operation");

            LogServiceBusDependency(logger, topicName, isSuccessful, startTime, duration, dependencyId, ServiceBusEntityType.Topic, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="entityName">Name of the Service Bus entity</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="entityType">Type of the Service Bus entity</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Service Bus dependency")]
        public static void LogServiceBusDependency(
            this ILogger logger,
            string entityName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires a non-blank Azure Service Bus entity name to track an Azure Service Bus dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus when tracking the Azure Service Bus dependency");

            LogServiceBusDependency(logger, entityName, isSuccessful, measurement.StartTime, measurement.Elapsed, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="entityName">The name of the Service Bus entity.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Service Bus dependency.</param>
        /// <param name="entityType">The type of the Service Bus entity.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> is blank.</exception>
        public static void LogServiceBusDependency(
            this ILogger logger,
            string entityName,
            bool isSuccessful,
            DurationMeasurement measurement,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires a non-blank Azure Service Bus entity name to track an Azure Service Bus dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus when tracking the Azure Service Bus dependency");

            LogServiceBusDependency(logger, entityName, isSuccessful, measurement.StartTime, measurement.Elapsed, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="entityName">The name of the Service Bus entity.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Service Bus dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="entityType">The type of the Service Bus entity.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> is blank.</exception>
        public static void LogServiceBusDependency(
            this ILogger logger,
            string entityName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires a non-blank Azure Service Bus entity name to track an Azure Service Bus dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Service Bus when tracking the Azure Service Bus dependency");

            LogServiceBusDependency(logger, entityName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="entityName">Name of the Service Bus entity</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="entityType">Type of the Service Bus entity</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusDependency(
            this ILogger logger,
            string entityName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires a non-blank Azure Service Bus entity name to track an Azure Service Bus dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus operation");

            context = context ?? new Dictionary<string, object>();

            LogServiceBusDependency(logger, entityName, isSuccessful, startTime, duration, dependencyId: null, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="entityName">Name of the Service Bus entity</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="entityType">Type of the Service Bus entity</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entityName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogServiceBusDependency(
            this ILogger logger,
            string entityName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            ServiceBusEntityType entityType = ServiceBusEntityType.Unknown,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires a non-blank Azure Service Bus entity name to track an Azure Service Bus dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus operation");

            context = context ?? new Dictionary<string, object>();
            context[ContextProperties.DependencyTracking.ServiceBus.EntityType] = entityType;

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
