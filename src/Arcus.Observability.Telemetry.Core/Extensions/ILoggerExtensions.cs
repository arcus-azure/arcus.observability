using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using GuardNet;
using static Arcus.Observability.Telemetry.Core.MessageFormats;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Telemetry extensions on the <see cref="ILogger"/> instance to write Application Insights compatible log messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Logs an HTTP request
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="response"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpResponseMessage response,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.NotNull(request.RequestUri, nameof(request.RequestUri), "Requires a request URI to track a HTTP request");
            Guard.For<ArgumentException>(() => request.RequestUri.Scheme?.Contains(" ") == true, "Requires a HTTP request scheme without whitespace");
            Guard.For<ArgumentException>(() => request.RequestUri.Host?.Contains(" ") == true, "Requires a HTTP request host name without whitespace");
            Guard.NotLessThan((int) response.StatusCode, 0, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan((int) response.StatusCode, 999, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            LogRequest(logger, request, response.StatusCode, duration, context);
        }

        /// <summary>
        /// Logs an HTTP request
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="response"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="response"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string operationName,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(response, nameof(response), "Requires a HTTP response instance to track a HTTP request");
            Guard.NotNull(request.RequestUri, nameof(request.RequestUri), "Requires a request URI to track a HTTP request");
            Guard.For<ArgumentException>(() => request.RequestUri.Scheme?.Contains(" ") == true, "Requires a HTTP request scheme without whitespace");
            Guard.For<ArgumentException>(() => request.RequestUri.Host?.Contains(" ") == true, "Requires a HTTP request host name without whitespace");
            Guard.NotLessThan((int)response.StatusCode, 0, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan((int)response.StatusCode, 999, nameof(response), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            LogRequest(logger, request, response.StatusCode, operationName, duration, context);
        }

        /// <summary>
        /// Logs an HTTP request
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="responseStatusCode"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode responseStatusCode,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(request.RequestUri, nameof(request.RequestUri), "Requires a request URI to track a HTTP request");
            Guard.For<ArgumentException>(() => request.RequestUri.Scheme?.Contains(" ") == true, "Requires a HTTP request scheme without whitespace");
            Guard.For<ArgumentException>(() => request.RequestUri.Host?.Contains(" ") == true, "Requires a HTTP request host name without whitespace");
            Guard.NotLessThan((int) responseStatusCode, 0, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan((int) responseStatusCode, 999, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            context = context ?? new Dictionary<string, object>();

            var statusCode = (int)responseStatusCode;
            string resourcePath = request.RequestUri.AbsolutePath;
            string host = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}";

            logger.LogWarning(RequestFormat, new RequestLogEntry(request.Method.ToString(), host, resourcePath, statusCode, duration, context));
        }

        /// <summary>
        /// Logs an HTTP request
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/>'s URI is blank,
        ///     the <paramref name="request"/>'s scheme contains whitespace,
        ///     the <paramref name="request"/>'s host contains whitespace,
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="responseStatusCode"/>'s status code is outside the 0-999 inclusively,
        ///     the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogRequest(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode responseStatusCode,
            string operationName,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request instance to track a HTTP request");
            Guard.NotNull(request.RequestUri, nameof(request.RequestUri), "Requires a request URI to track a HTTP request");
            Guard.For<ArgumentException>(() => request.RequestUri.Scheme?.Contains(" ") == true, "Requires a HTTP request scheme without whitespace");
            Guard.For<ArgumentException>(() => request.RequestUri.Host?.Contains(" ") == true, "Requires a HTTP request host name without whitespace");
            Guard.NotLessThan((int)responseStatusCode, 0, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotGreaterThan((int)responseStatusCode, 999, nameof(responseStatusCode), "Requires a HTTP response status code that's within the 0-999 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            context = context ?? new Dictionary<string, object>();

            var statusCode = (int)responseStatusCode;
            string resourcePath = request.RequestUri.AbsolutePath;
            string host = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}";

            logger.LogWarning(RequestFormat, new RequestLogEntry(request.Method.ToString(), host, resourcePath, operationName, statusCode, duration, context));
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

            logger.LogWarning(RequestFormat, RequestLogEntry.CreateForServiceBus(operationName, isSuccessful, duration, startTime, context));
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a custom dependency")]
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="dependencyName">The name of the dependency</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a custom dependency")]
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the dependency operation");
            
            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful: isSuccessful, startTime: startTime, duration: duration, context: context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="dependencyName">The name of the dependency</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            bool isSuccessful,
            string dependencyName,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the dependency operation");

            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful: isSuccessful, dependencyName, startTime: startTime, duration: duration, context: context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="targetName">Name of the dependency target</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a custom dependency")]
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");
            
            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of the dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="targetName">Name of the dependency target</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="dependencyName">The name of the dependency</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a custom dependency")]
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of the dependency target.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/>, <paramref name="measurement"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the dependency when tracking the custom dependency");

            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="targetName">Name of dependency target</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the dependency operation");

            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, dependencyName: targetName, startTime, duration, context);
        }

        /// <summary>
        /// Logs a dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="targetName">Name of dependency target</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="dependencyName">The name of the dependency</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, <paramref name="dependencyData"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogDependency(
            this ILogger logger,
            string dependencyType,
            object dependencyData,
            string targetName,
            bool isSuccessful,
            string dependencyName,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotNull(dependencyData, nameof(dependencyData), "Requires custom dependency data when tracking the custom dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the dependency operation");

            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType,
                dependencyName: dependencyName,
                dependencyData: dependencyData,
                targetName: targetName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Key Vault dependency")]
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Key Vault dependency")]
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Key Vault operation");
            
            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Key Vault operation");
            
            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure key vault", 
                dependencyName: vaultUri, 
                dependencyData: secretName, 
                targetName: vaultUri, 
                duration: duration, 
                dependencyId: dependencyId,
                startTime: startTime, 
                resultCode: null,
                isSuccessful: isSuccessful, 
                context: context));
        }

        /// <summary>
        /// Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="searchServiceName">Name of the Azure Search service</param>
        /// <param name="operationName">Name of the operation to execute on the Azure Search service</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="searchServiceName"/> or <paramref name="operationName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure search dependency")]
        public static void LogAzureSearchDependency(
            this ILogger logger,
            string searchServiceName,
            string operationName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track the Azure Search dependency");
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name for the Azure Search service to track the Azure Service dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Search resource when tracking the Azure Search dependency");
            
            context = context ?? new Dictionary<string, object>();

            LogAzureSearchDependency(logger, searchServiceName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="searchServiceName">The name of the Azure Search service.</param>
        /// <param name="operationName">The name of the operation to execute on the Azure Search service.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
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

            context = context ?? new Dictionary<string, object>();

            LogAzureSearchDependency(logger, searchServiceName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
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

            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure Search",
                dependencyName: searchServiceName,
                dependencyData: operationName,
                targetName: searchServiceName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }

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
            context[ContextProperties.DependencyTracking.ServiceBus.EntityType] = entityType;

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure Service Bus", 
                dependencyName: entityName, 
                dependencyData: null, 
                targetName: entityName, 
                duration: duration, 
                startTime: startTime, 
                resultCode: null,
                isSuccessful: isSuccessful, 
                context: context));
        }

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

            string dependencyName = $"{accountName}/{containerName}";

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure blob", 
                dependencyName: dependencyName, 
                dependencyData: containerName, 
                targetName: accountName, 
                duration: duration, 
                startTime: startTime, 
                resultCode: null,
                isSuccessful: isSuccessful, 
                context: context));
        }

        /// <summary>
        /// Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="tableName">Name of the Table Storage resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Table Storage dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="tableName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Table storage dependency")]
        public static void LogTableStorageDependency(
            this ILogger logger, 
            string accountName, 
            string tableName, 
            bool isSuccessful, 
            DependencyMeasurement measurement, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Table storage resource to track an Azure Table storage dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank table name in the Azure Table storage resource to track an Azure Table storage dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Table storage when tracking an Azure Table storage dependency");

            LogTableStorageDependency(logger, accountName, tableName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The account of the storage resource.</param>
        /// <param name="tableName">The name of the Table Storage resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the Table Storage dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/> or <paramref name="tableName"/> is blank.</exception>
        public static void LogTableStorageDependency(
            this ILogger logger,
            string accountName,
            string tableName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Table storage resource to track an Azure Table storage dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank table name in the Azure Table storage resource to track an Azure Table storage dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Table storage when tracking an Azure Table storage dependency");

            LogTableStorageDependency(logger, accountName, tableName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
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
        public static void LogTableStorageDependency(
            this ILogger logger, 
            string accountName, 
            string tableName, 
            bool isSuccessful, 
            DateTimeOffset startTime, 
            TimeSpan duration, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name for the Azure Table storage resource to track an Azure Table storage dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank table name in the Azure Table storage resource to track an Azure Table storage dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Table storage operation");
            
            context = context ?? new Dictionary<string, object>();

            string dependencyName = $"{accountName}/{tableName}";

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure table",
                dependencyName: dependencyName,
                dependencyData: tableName,
                targetName: accountName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }

        /// <summary>
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">Namespace of the resource</param>
        /// <param name="eventHubName">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Event Hubs dependency")]
        public static void LogEventHubsDependency(
            this ILogger logger, 
            string namespaceName, 
            string eventHubName, 
            bool isSuccessful, 
            DependencyMeasurement measurement, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Event Hub resource when tracking an Azure Event Hub dependency");
            
            LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">The namespace of the resource.</param>
        /// <param name="eventHubName">The name of the Event Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        public static void LogEventHubsDependency(
            this ILogger logger,
            string namespaceName,
            string eventHubName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Event Hub resource when tracking an Azure Event Hub dependency");

            LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="namespaceName">Namespace of the resource</param>
        /// <param name="eventHubName">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="namespaceName"/> or <paramref name="eventHubName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogEventHubsDependency(
            this ILogger logger, 
            string namespaceName, 
            string eventHubName, 
            bool isSuccessful, 
            DateTimeOffset startTime, 
            TimeSpan duration, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName), "Requires a non-blank resource namespace of the Azure Event Hub to track an Azure Event Hub dependency");
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName), "Requires a non-blank Azure Event Hub name to track an Azure Event Hub dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Events Hubs operation");
            
            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure Event Hubs",
                dependencyName: eventHubName,
                dependencyData: namespaceName,
                targetName: eventHubName,
                duration: duration,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">Name of the IoT Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure IoT Hub dependency")]
        public static void LogIotHubDependency(
            this ILogger logger, 
            string iotHubName, 
            bool isSuccessful, 
            DependencyMeasurement measurement, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");

            LogIotHubDependency(logger, iotHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">The nme of the IoT Hub resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        public static void LogIotHubDependency(
            this ILogger logger,
            string iotHubName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the IoT Hub resource when tracking a IoT Hub dependency");

            LogIotHubDependency(logger, iotHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="iotHubName">Name of the IoT Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="iotHubName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogIotHubDependency(
            this ILogger logger, 
            string iotHubName, 
            bool isSuccessful, 
            DateTimeOffset startTime, 
            TimeSpan duration, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName), "Requires a non-blank resource name of the IoT Hub resource to track a IoT Hub dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the IoT Hub operation");
            
            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure IoT Hub", 
                dependencyName: iotHubName, 
                dependencyData: null, 
                targetName: iotHubName, 
                duration: duration, 
                startTime: startTime, 
                resultCode: null, 
                isSuccessful: isSuccessful, 
                context: context));
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Name of the storage resource</param>
        /// <param name="database">Name of the database</param>
        /// <param name="container">Name of the container</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency of the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a Cosmos SQL dependency")]
        public static void LogCosmosSqlDependency(
            this ILogger logger, 
            string accountName, 
            string database, 
            string container, 
            bool isSuccessful, 
            DependencyMeasurement measurement, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Cosmos SQL storage when tracking an Cosmos SQL dependency");
            
            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">The name of the storage resource.</param>
        /// <param name="database">The name of the database.</param>
        /// <param name="container">The name of the container.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful</param>
        /// <param name="measurement">The measuring the latency of the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        public static void LogCosmosSqlDependency(
            this ILogger logger,
            string accountName,
            string database,
            string container,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Cosmos SQL storage when tracking an Cosmos SQL dependency");

            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="accountName">Name of the storage resource</param>
        /// <param name="database">Name of the database</param>
        /// <param name="container">Name of the container</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="accountName"/>, <paramref name="database"/>, or <paramref name="container"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogCosmosSqlDependency(
            this ILogger logger, 
            string accountName, 
            string database, 
            string container, 
            bool isSuccessful, 
            DateTimeOffset startTime, 
            TimeSpan duration, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(accountName, nameof(accountName), "Requires a non-blank account name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(database, nameof(database), "Requires a non-blank database name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotNullOrWhitespace(container, nameof(container), "Requires a non-blank container name of the Cosmos SQL storage to track a Cosmos SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Cosmos SQL operation");
            
            context = context ?? new Dictionary<string, object>();
            string data = $"{database}/{container}";

            logger.LogWarning(DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure DocumentDB", 
                dependencyName: data, 
                dependencyData: data, 
                targetName: accountName, 
                duration: duration, 
                startTime: startTime, 
                resultCode: null, 
                isSuccessful: 
                isSuccessful, 
                context: context));
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that started the HTTP communication</param>
        /// <param name="statusCode">Status code that was returned by the service for this HTTP communication</param>
        /// <param name="measurement">Measuring the latency of the HTTP dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a HTTP dependency")]
        public static void LogHttpDependency(
            this ILogger logger, 
            HttpRequestMessage request, 
            HttpStatusCode statusCode, 
            DependencyMeasurement measurement, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode), 
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));
            
            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">The request that started the HTTP communication.</param>
        /// <param name="statusCode">The status code that was returned by the service for this HTTP communication.</param>
        /// <param name="measurement">The measuring the latency of the HTTP dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="request"/>, or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger,
            HttpRequestMessage request,
            HttpStatusCode statusCode,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the HTTP communication when tracking a HTTP dependency");
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode),
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="request">Request that started the HTTP communication</param>
        /// <param name="statusCode">Status code that was returned by the service for this HTTP communication</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="request"/> doesn't have a request URI or HTTP method, the <paramref name="statusCode"/> is outside the bounds of the enumeration.
        /// </exception>
        public static void LogHttpDependency(
            this ILogger logger, 
            HttpRequestMessage request, 
            HttpStatusCode statusCode, 
            DateTimeOffset startTime, 
            TimeSpan duration, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNull(request, nameof(request), "Requires a HTTP request message to track a HTTP dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the HTTP dependency operation");
            Guard.For(() => request.RequestUri is null, new ArgumentException("Requires a HTTP request URI to track a HTTP dependency", nameof(request)));
            Guard.For(() => request.Method is null, new ArgumentException("Requires a HTTP request method to track a HTTP dependency", nameof(request)));
            Guard.For(() => !Enum.IsDefined(typeof(HttpStatusCode), statusCode), 
                new ArgumentException("Requires a response HTTP status code that's within the bound of the enumeration to track a HTTP dependency"));
            
            context = context ?? new Dictionary<string, object>();

            Uri requestUri = request.RequestUri;
            string targetName = requestUri.Host;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            bool isSuccessful = (int) statusCode >= 200 && (int) statusCode < 300;

            logger.LogWarning(HttpDependencyFormat, new DependencyLogEntry(
                dependencyType: "Http",
                dependencyName: dependencyName, 
                dependencyData: null,
                targetName: targetName,
                duration: duration, 
                startTime: startTime, 
                resultCode: (int) statusCode, 
                isSuccessful: isSuccessful,
                context: context));
        }

        /// <summary>
        /// Logs a SQL dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">Name of server hosting the database</param>
        /// <param name="databaseName">Name of database</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the SQL dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, or <paramref name="tableName"/> is blank.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track a SQL dependency")]
        public static void LogSqlDependency(
            this ILogger logger, 
            string serverName, 
            string databaseName, 
            string tableName, 
            bool isSuccessful, 
            DependencyMeasurement measurement, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, tableName, measurement.DependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a SQL dependency.
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">The name of server hosting the database.</param>
        /// <param name="databaseName">The name of database.</param>
        /// <param name="tableName">The name of tracked table in the SQL database.</param>
        /// <param name="operationName">The name of the operation that was performed on the SQL database.</param>	
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measuring the latency to call the SQL dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, <paramref name="tableName"/>, or <paramref name="operationName"/> is blank.
        /// </exception>
        public static void LogSqlDependency(
            this ILogger logger,
            string serverName,
            string databaseName,
            string tableName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name of the SQL operation to track a SQL dependency");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to measure the latency of the SQL storage when tracking an SQL dependency");

            LogSqlDependency(logger, serverName, databaseName, tableName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs a SQL dependency
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="serverName">Name of server hosting the database</param>
        /// <param name="databaseName">Name of database</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="operationName">Name of the operation that was performed</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="serverName"/>, <paramref name="databaseName"/>, <paramref name="tableName"/>, or <paramref name="operationName"/> is blank.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogSqlDependency(
            this ILogger logger, 
            string serverName, 
            string databaseName, 
            string tableName, 
            string operationName, 
            bool isSuccessful, 
            DateTimeOffset startTime, 
            TimeSpan duration, 
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(serverName, nameof(serverName), "Requires a non-blank SQL server name to track a SQL dependency");
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName), "Requires a non-blank SQL database name to track a SQL dependency");
            Guard.NotNullOrWhitespace(tableName, nameof(tableName), "Requires a non-blank SQL table name to track a SQL dependency");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank name of the SQL operation to track a SQL dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the SQL dependency operation");
            
            context = context ?? new Dictionary<string, object>();

            string dependencyName = $"{databaseName}/{tableName}";

            logger.LogWarning(SqlDependencyFormat, new DependencyLogEntry(
                dependencyType: "Sql",
                targetName: serverName,
                dependencyName: dependencyName, 
                dependencyData: operationName, 
                duration: duration, 
                startTime: startTime, 
                resultCode: null,
                isSuccessful: isSuccessful, 
                context: context));
        }

        /// <summary>
        /// Logs a custom event
        /// </summary>
        /// <param name="logger">The logger to track the telemetry.</param>
        /// <param name="name">Name of the event</param>
        /// <param name="context">Context that provides more insights on the event that occurred</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public static void LogEvent(this ILogger logger, string name, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(name, nameof(name), "Requires a non-blank event name to track an custom event");

            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(EventFormat, new EventLogEntry(name, context));
        }

        /// <summary>
        /// Logs a custom metric
        /// </summary>
        /// <param name="logger">The logger to track the metric.</param>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="context">Context that provides more insights on the event that occurred</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public static void LogMetric(this ILogger logger, string name, double value, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(name, nameof(name), "Requires a non-blank name to track a metric");

            context = context ?? new Dictionary<string, object>();

            LogMetric(logger, name, value, DateTimeOffset.UtcNow, context);
        }

        /// <summary>
        /// Logs a custom metric
        /// </summary>
        /// <param name="logger">The logger to track the metric.</param>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="timestamp">Timestamp of the metric</param>
        /// <param name="context">Context that provides more insights on the event that occurred</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public static void LogMetric(this ILogger logger, string name, double value, DateTimeOffset timestamp, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(name, nameof(name), "Requires a non-blank name to track a metric");

            context = context ?? new Dictionary<string, object>();

            logger.LogWarning(MetricFormat, new MetricLogEntry(name, value, timestamp, context));
        }

        /// <summary>
        /// Logs an event related to an security activity (i.e. input validation, authentication, authorization...).
        /// </summary>
        /// <param name="logger">The logger to track the security event.</param>
        /// <param name="name">The name of the security event written.</param>
        /// <param name="context">The context that provides more insights on the event that occurred.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public static void LogSecurityEvent(this ILogger logger, string name, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a logger instance to track telemetry");
            Guard.NotNullOrWhitespace(name, nameof(name), "Requires a non-blank name of the event to track an security event");

            context = context ?? new Dictionary<string, object>();
            context["EventType"] = "Security";

            LogEvent(logger, name, context);
        }
    }
}
