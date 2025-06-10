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
        /// Logs an Azure Service Bus topic request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus topic is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus topic is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="topicName">The name of the Azure Service Bus topic.</param>
        /// <param name="subscriptionName">The name of the subscription on the Azure Service Bus topic.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus topic request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure Service Bus topic request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus topic request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serviceBusNamespace"/>, <paramref name="serviceBusNamespaceSuffix"/>, <paramref name="topicName"/>, or the <paramref name="subscriptionName"/> is blank.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusTopicRequestWithSuffix(
            this ILogger logger,
            string serviceBusNamespace,
            string serviceBusNamespaceSuffix,
            string topicName,
            string subscriptionName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogServiceBusTopicRequestWithSuffix(logger, serviceBusNamespace, serviceBusNamespaceSuffix, topicName, subscriptionName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus topic request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus topic is registered (use Azure public cloud).</param>
        /// <param name="topicName">The name of the Azure Service Bus topic.</param>
        /// <param name="subscriptionName">The name of the subscription on the Azure Service Bus topic.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus topic request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure Service Bus topic request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus topic request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serviceBusNamespace"/>, <paramref name="topicName"/>, or the <paramref name="subscriptionName"/> is blank.
        /// </exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusTopicRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string topicName,
            string subscriptionName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogServiceBusTopicRequest(logger, serviceBusNamespace, topicName, subscriptionName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus topic request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus topic is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus topic is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="topicName">The name of the Azure Service Bus topic.</param>
        /// <param name="subscriptionName">The name of the subscription on the Azure Service Bus topic.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus topic request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus topic request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus topic request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serviceBusNamespace"/>, <paramref name="serviceBusNamespaceSuffix"/>, <paramref name="topicName"/>, or the <paramref name="subscriptionName"/> is blank.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusTopicRequestWithSuffix(
            this ILogger logger,
            string serviceBusNamespace,
            string serviceBusNamespaceSuffix,
            string topicName,
            string subscriptionName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            Dictionary<string, object> context = null)
        {
            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);
            context[ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName] = subscriptionName;

            LogServiceBusRequestWithSuffix(logger, serviceBusNamespace, serviceBusNamespaceSuffix, topicName, operationName, isSuccessful, duration, startTime, ServiceBusEntityType.Topic, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus topic request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace prefix where the Azure Service Bus topic is registered (use Azure public cloud).</param>
        /// <param name="topicName">The name of the Azure Service Bus topic.</param>
        /// <param name="subscriptionName">The name of the subscription on the Azure Service Bus topic.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus topic request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus topic request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus topic request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/>, <paramref name="topicName"/> or the <paramref name="subscriptionName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusTopicRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string topicName,
            string subscriptionName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            Dictionary<string, object> context = null)
        {
            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);
            context[ContextProperties.RequestTracking.ServiceBus.Topic.SubscriptionName] = subscriptionName;

            LogServiceBusRequest(logger, serviceBusNamespace, topicName, operationName, isSuccessful, duration, startTime, ServiceBusEntityType.Topic, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus queue request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus queue is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus queue is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="queueName">The name of the Azure Service Bus queue.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus queue request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure Service Bus queue request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus queue request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="queueName"/> is blank.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusQueueRequestWithSuffix(
            this ILogger logger,
            string serviceBusNamespace,
            string serviceBusNamespaceSuffix,
            string queueName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogServiceBusQueueRequestWithSuffix(logger, serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus queue request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace prefix where the Azure Service Bus queue is registered (use Azure public cloud).</param>
        /// <param name="queueName">The name of the Azure Service Bus queue.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus queue request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure Service Bus queue request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus queue request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="queueName"/> is blank.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusQueueRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string queueName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogServiceBusQueueRequest(logger, serviceBusNamespace, queueName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus queue request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus queue is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus queue is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="queueName">The name of the Azure Service Bus queue.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus queue request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus queue request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus queue request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="queueName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusQueueRequestWithSuffix(
            this ILogger logger,
            string serviceBusNamespace,
            string serviceBusNamespaceSuffix,
            string queueName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            Dictionary<string, object> context = null)
        {
            LogServiceBusRequestWithSuffix(logger, serviceBusNamespace, serviceBusNamespaceSuffix, queueName, operationName, isSuccessful, duration, startTime, ServiceBusEntityType.Queue, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus queue request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace prefix where the Azure Service Bus queue is registered (use Azure public cloud).</param>
        /// <param name="queueName">The name of the Azure Service Bus queue.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus queue request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus queue request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus queue request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="queueName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusQueueRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string queueName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            Dictionary<string, object> context = null)
        {
            LogServiceBusRequest(logger, serviceBusNamespace, queueName, operationName, isSuccessful, duration, startTime, ServiceBusEntityType.Queue, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus queue is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus queue is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="entityName">The name of the Azure Service Bus entity.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure Service Bus queue request.</param>
        /// <param name="entityType">The type of the Azure Service Bus entity.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="entityName"/> is blank.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusRequestWithSuffix(
            this ILogger logger,
            string serviceBusNamespace,
            string serviceBusNamespaceSuffix,
            string entityName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            ServiceBusEntityType entityType,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogServiceBusRequestWithSuffix(logger, serviceBusNamespace, serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace prefix where the Azure Service Bus is registered (use Azure public cloud).</param>
        /// <param name="entityName">The name of the Azure Service Bus entity.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure Service Bus queue request.</param>
        /// <param name="entityType">The type of the Azure Service Bus entity.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="entityName"/> is blank.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string entityName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            ServiceBusEntityType entityType,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogServiceBusRequest(logger, serviceBusNamespace, entityName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="entityName">The name of the Azure Service Bus entity.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="entityType">The type of the Azure Service Bus entity.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="entityName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusRequestWithSuffix(
            this ILogger logger,
            string serviceBusNamespace,
            string serviceBusNamespaceSuffix,
            string entityName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            ServiceBusEntityType entityType,
            Dictionary<string, object> context = null)
        {
            LogServiceBusRequest(logger, serviceBusNamespace + serviceBusNamespaceSuffix, entityName, operationName, isSuccessful, duration, startTime, entityType, context);
        }

        /// <summary>
        /// Logs an Azure Service Bus request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace prefix where the Azure Service Bus is registered (use Azure public cloud).</param>
        /// <param name="entityName">The name of the Azure Service Bus entity.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure Service Bus request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="entityType">The type of the Azure Service Bus entity.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="entityName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogServiceBusRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string entityName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            ServiceBusEntityType entityType,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (string.IsNullOrWhiteSpace(serviceBusNamespace))
            {
                throw new ArgumentException("Requires an Azure Service Bus namespace to track the request", nameof(serviceBusNamespace));
            }

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentException("Requires an Azure Service Bus name to track the request", nameof(entityName));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the Azure Service Bus request operation");
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                operationName = ContextProperties.RequestTracking.ServiceBus.DefaultOperationName;
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);
            context[ContextProperties.RequestTracking.ServiceBus.Endpoint] = serviceBusNamespace;
            context[ContextProperties.RequestTracking.ServiceBus.EntityName] = entityName;
            context[ContextProperties.RequestTracking.ServiceBus.EntityType] = entityType;

            logger.LogWarning(MessageFormats.RequestFormat, RequestLogEntry.CreateForServiceBus(operationName, isSuccessful, duration, startTime, context));
        }
    }
}
