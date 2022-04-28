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
        /// Logs an Azure Service Bus topic request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="serviceBusNamespace">The namespace where the Azure Service Bus topic is registered. (Will be combined with <paramref name="serviceBusNamespaceSuffix"/>).</param>
        /// <param name="serviceBusNamespaceSuffix">The namespace suffix (i.e. '.servicebus.windows.net' or '*.servicebus.cloudapi.de') where the Azure Service Bus topic is registered (Will be combined with <paramref name="serviceBusNamespace"/>).</param>
        /// <param name="topicName">The name of the Azure Service Bus topic.</param>
        /// <param name="subscriptionName">The name of the subscription on the Azure Service Bus topic.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure Service Bus topic request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the latency duration of the Azure Service Bus topic request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus topic request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serviceBusNamespace"/>, <paramref name="serviceBusNamespaceSuffix"/>, <paramref name="topicName"/>, or the <paramref name="subscriptionName"/> is blank.
        /// </exception>
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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the topic request");
            Guard.NotNullOrWhitespace(serviceBusNamespaceSuffix, nameof(serviceBusNamespaceSuffix), "Requires an Azure Service Bus namespace suffix to track the topic request");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires an Azure Service Bus topic name to track the topic request");
            Guard.NotNullOrWhitespace(subscriptionName, nameof(subscriptionName), "Requires an Azure Service Bus subscription name on the to track the topic request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure Service Bus topic request process latency duration");

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
        /// <param name="measurement">The instance to measure the latency duration of the Azure Service Bus topic request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus topic request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serviceBusNamespace"/>, <paramref name="topicName"/>, or the <paramref name="subscriptionName"/> is blank.
        /// </exception>
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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the topic request");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires an Azure Service Bus topic name to track the topic request");
            Guard.NotNullOrWhitespace(subscriptionName, nameof(subscriptionName), "Requires an Azure Service Bus subscription name on the to track the topic request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure Service Bus topic request process latency duration");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the topic request");
            Guard.NotNullOrWhitespace(serviceBusNamespaceSuffix, nameof(serviceBusNamespaceSuffix), "Requires an Azure Service Bus namespace suffix to track the topic request");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires an Azure Service Bus topic name to track the topic request");
            Guard.NotNullOrWhitespace(subscriptionName, nameof(subscriptionName), "Requires an Azure Service Bus subscription name on the to track the topic request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus topic request operation");

            context = context ?? new Dictionary<string, object>();
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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the topic request");
            Guard.NotNullOrWhitespace(topicName, nameof(topicName), "Requires an Azure Service Bus topic name to track the topic request");
            Guard.NotNullOrWhitespace(subscriptionName, nameof(subscriptionName), "Requires an Azure Service Bus subscription name on the to track the topic request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus topic request operation");

            context = context ?? new Dictionary<string, object>();
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
        /// <param name="measurement">The instance to measure the latency duration of the Azure Service Bus queue request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus queue request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="queueName"/> is blank.</exception>
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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the queue request");
            Guard.NotNullOrWhitespace(serviceBusNamespaceSuffix, nameof(serviceBusNamespaceSuffix), "Requires an Azure Service Bus namespace suffix to track the queue request");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires an Azure Service Bus queue name to track the queue request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure Service Bus queue request process latency duration");

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
        /// <param name="measurement">The instance to measure the latency duration of the Azure Service Bus queue request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus queue request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="queueName"/> is blank.</exception>
        public static void LogServiceBusQueueRequest(
            this ILogger logger,
            string serviceBusNamespace,
            string queueName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the queue request");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires an Azure Service Bus queue name to track the queue request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure Service Bus queue request process latency duration");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the queue request");
            Guard.NotNullOrWhitespace(serviceBusNamespaceSuffix, nameof(serviceBusNamespaceSuffix), "Requires an Azure Service Bus namespace suffix to track the queue request");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires an Azure Service Bus queue name to track the queue request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus queue request operation");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the queue request");
            Guard.NotNullOrWhitespace(queueName, nameof(queueName), "Requires an Azure Service Bus queue name to track the queue request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus queue request operation");

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
        /// <param name="measurement">The instance to measure the latency duration of the Azure Service Bus queue request.</param>
        /// <param name="entityType">The type of the Azure Service Bus entity.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="entityName"/> is blank.</exception>
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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the queue request");
            Guard.NotNullOrWhitespace(serviceBusNamespaceSuffix, nameof(serviceBusNamespaceSuffix), "Requires an Azure Service Bus namespace suffix to track the queue request");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires an Azure Service Bus name to track the request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure Service Bus request process latency duration");

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
        /// <param name="measurement">The instance to measure the latency duration of the Azure Service Bus queue request.</param>
        /// <param name="entityType">The type of the Azure Service Bus entity.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure Service Bus request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceBusNamespace"/> or <paramref name="entityName"/> is blank.</exception>
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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the queue request");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires an Azure Service Bus name to track the request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure Service Bus request process latency duration");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the request");
            Guard.NotNullOrWhitespace(serviceBusNamespaceSuffix, nameof(serviceBusNamespaceSuffix), "Requires an Azure Service Bus namespace suffix to track the request");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires an Azure Service Bus name to track the request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus request operation");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serviceBusNamespace, nameof(serviceBusNamespace), "Requires an Azure Service Bus namespace to track the request");
            Guard.NotNullOrWhitespace(entityName, nameof(entityName), "Requires an Azure Service Bus name to track the request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Service Bus request operation");

            if (string.IsNullOrWhiteSpace(operationName))
            {
                operationName = ContextProperties.RequestTracking.ServiceBus.DefaultOperationName;
            }

            context = context ?? new Dictionary<string, object>();
            context[ContextProperties.RequestTracking.ServiceBus.Endpoint] = serviceBusNamespace;
            context[ContextProperties.RequestTracking.ServiceBus.EntityName] = entityName;
            context[ContextProperties.RequestTracking.ServiceBus.EntityType] = entityType;

            logger.LogWarning(MessageFormats.RequestFormat, RequestLogEntry.CreateForServiceBus(operationName, isSuccessful, duration, startTime, context));
        }
    }
}
