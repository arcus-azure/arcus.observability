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
        /// Logs an Azure EventHubs request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="eventHubsNamespace">The namespace in which the Azure EventHUbs is located.</param>
        /// <param name="eventHubsName">The name of the Event Hub that the processor is connected to, specific to the EventHubs namespace that contains it.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure EventHubs request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure EventHubs request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="eventHubsNamespace"/>, <paramref name="eventHubsName"/> is blank.</exception>
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string eventHubsName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(eventHubsNamespace, nameof(eventHubsNamespace), "Requires an Azure EventHubs namespace to track the request");
            Guard.NotNullOrWhitespace(eventHubsName, nameof(eventHubsName), "Requires an Azure EventHubs name to track the request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure EventHubs request process latency duration");

            LogEventHubsRequest(logger, eventHubsNamespace, eventHubsName, isSuccessful, measurement.Elapsed, measurement.StartTime, context);
        }

        /// <summary>
        /// Logs an Azure EventHubs request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="eventHubsNamespace">The namespace in which the Azure EventHUbs is located.</param>
        /// <param name="eventHubsName">The name of the Event Hub that the processor is connected to, specific to the EventHubs namespace that contains it.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure EventHubs request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure EventHubs request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="eventHubsNamespace"/>, <paramref name="eventHubsName"/> is blank.</exception>
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string eventHubsName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(eventHubsNamespace, nameof(eventHubsNamespace), "Requires an Azure EventHubs namespace to track the request");
            Guard.NotNullOrWhitespace(eventHubsName, nameof(eventHubsName), "Requires an Azure EventHubs name to track the request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure EventHubs request operation");

            LogEventHubsRequest(logger, eventHubsNamespace, "$Default", eventHubsName, operationName: null, isSuccessful, duration, startTime, context);
        }

        /// <summary>
        /// Logs an Azure EventHubs request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="eventHubsNamespace">The namespace in which the Azure EventHUbs is located.</param>
        /// <param name="consumerGroup">The name of the consumer group this processor is associated with. Events are read in the context of this group.</param>
        /// <param name="eventHubsName">The name of the Event Hub that the processor is connected to, specific to the EventHubs namespace that contains it.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure EventHubs request was successfully processed.</param>
        /// <param name="measurement">The instance to measure the duration of the Azure EventHubs request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="eventHubsNamespace"/>, <paramref name="consumerGroup"/>, <paramref name="eventHubsName"/> is blank.
        /// </exception>
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string consumerGroup,
            string eventHubsName,
            string operationName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(eventHubsNamespace, nameof(eventHubsNamespace), "Requires an Azure EventHubs namespace to track the request");
            Guard.NotNullOrWhitespace(consumerGroup, nameof(consumerGroup), "Requires an Azure EventHubs consumer group to track the request");
            Guard.NotNullOrWhitespace(eventHubsName, nameof(eventHubsName), "Requires an Azure EventHubs name to track the request");
            Guard.NotNull(measurement, nameof(measurement), "Requires an instance to measure the Azure EventHubs request process latency duration");

            LogEventHubsRequest(logger, eventHubsNamespace, consumerGroup, eventHubsName, operationName, isSuccessful, measurement.Elapsed, measurement.StartTime, context);
        }

        /// <summary>
        /// Logs an Azure EventHubs request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="eventHubsNamespace">The namespace in which the Azure EventHUbs is located.</param>
        /// <param name="consumerGroup">The name of the consumer group this processor is associated with. Events are read in the context of this group.</param>
        /// <param name="eventHubsName">The name of the Event Hub that the processor is connected to, specific to the EventHubs namespace that contains it.</param>
        /// <param name="operationName">The optional logical name that can be used to identify the operation that consumes the message.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure EventHubs request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the Azure EventHubs request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="eventHubsNamespace"/>, <paramref name="consumerGroup"/>, <paramref name="eventHubsName"/> is blank.
        /// </exception>
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string consumerGroup,
            string eventHubsName,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to track telemetry");
            Guard.NotNullOrWhitespace(eventHubsNamespace, nameof(eventHubsNamespace), "Requires an Azure EventHubs namespace to track the request");
            Guard.NotNullOrWhitespace(consumerGroup, nameof(consumerGroup), "Requires an Azure EventHubs consumer group to track the request");
            Guard.NotNullOrWhitespace(eventHubsName, nameof(eventHubsName), "Requires an Azure EventHubs name to track the request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure EventHubs request operation");

            if (string.IsNullOrWhiteSpace(operationName))
            {
                operationName = ContextProperties.RequestTracking.EventHubs.DefaultOperationName;
            }

            context = context ?? new Dictionary<string, object>();
            context[ContextProperties.RequestTracking.EventHubs.Namespace] = eventHubsNamespace;
            context[ContextProperties.RequestTracking.EventHubs.ConsumerGroup] = consumerGroup;
            context[ContextProperties.RequestTracking.EventHubs.Name] = eventHubsName;

            logger.LogWarning(MessageFormats.RequestFormat, RequestLogEntry.CreateForEventHubs(operationName, isSuccessful, duration, startTime, context));
        }
    }
}
