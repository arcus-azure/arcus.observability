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
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string eventHubsName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogEventHubsRequest(logger, eventHubsNamespace, eventHubsName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        /// Logs an Azure EventHubs request.
        /// </summary>
        /// <param name="logger">The logger instance to track the telemetry.</param>
        /// <param name="eventHubsNamespace">The namespace in which the Azure EventHUbs is located.</param>
        /// <param name="eventHubsName">The name of the Event Hub that the processor is connected to, specific to the EventHubs namespace that contains it.</param>
        /// <param name="isSuccessful">The indication whether or not the Azure EventHubs request was successfully processed.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="duration">The duration it took to process the Azure EventHubs request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="eventHubsNamespace"/>, <paramref name="eventHubsName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string eventHubsName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            LogEventHubsRequest(logger, eventHubsNamespace, "$Default", eventHubsName, operationName: null, isSuccessful: isSuccessful, startTime: startTime, duration: duration, context: context);
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
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
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
            if (measurement is null)
            {
                throw new ArgumentNullException(nameof(measurement));
            }

            LogEventHubsRequest(logger, eventHubsNamespace, consumerGroup, eventHubsName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
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
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="duration">The duration it took to process the Azure EventHubs request.</param>
        /// <param name="context">The telemetry context that provides more insights on the Azure EventHubs request.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="eventHubsNamespace"/>, <paramref name="consumerGroup"/>, <paramref name="eventHubsName"/> is blank.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        [Obsolete("Will be removed in v4.0 as the Azure SDK supports telemetry now natively")]
        public static void LogEventHubsRequest(
            this ILogger logger,
            string eventHubsNamespace,
            string consumerGroup,
            string eventHubsName,
            string operationName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (string.IsNullOrWhiteSpace(eventHubsNamespace))
            {
                throw new ArgumentException("Requires an Azure EventHubs namespace to track the request", nameof(eventHubsNamespace));
            }

            if (string.IsNullOrWhiteSpace(consumerGroup))
            {
                throw new ArgumentException("Requires an Azure EventHubs consumer group to track the request", nameof(consumerGroup));
            }

            if (string.IsNullOrWhiteSpace(eventHubsName))
            {
                throw new ArgumentException("Requires an Azure EventHubs name to track the request", nameof(eventHubsName));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Requires a positive time duration of the Azure EventHubs request operation");
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                operationName = ContextProperties.RequestTracking.EventHubs.DefaultOperationName;
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);
            context[ContextProperties.RequestTracking.EventHubs.Namespace] = eventHubsNamespace;
            context[ContextProperties.RequestTracking.EventHubs.ConsumerGroup] = consumerGroup;
            context[ContextProperties.RequestTracking.EventHubs.Name] = eventHubsName;

            logger.LogWarning(MessageFormats.RequestFormat, RequestLogEntry.CreateForEventHubs(operationName, isSuccessful, duration, startTime, context));
        }
    }
}
