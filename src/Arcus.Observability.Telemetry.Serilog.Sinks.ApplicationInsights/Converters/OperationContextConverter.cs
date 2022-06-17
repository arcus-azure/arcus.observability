using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using GuardNet;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from the Operation-related logging information to the Application Insights <see cref="OperationContext"/> instance.
    /// </summary>
    public class OperationContextConverter
    {
        private readonly ApplicationInsightsSinkOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationContextConverter" /> class.
        /// </summary>
        [Obsolete("Use the constructor overload with the Application Insights options instead")]
        public OperationContextConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationContextConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to influence the behavior of the Application Insights Serilog sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public OperationContextConverter(ApplicationInsightsSinkOptions options)
        {
            Guard.NotNull(options, nameof(options), "Requires a set of options to influence the behavior of the Application Insights Serilog sink");
            _options = options;
        }

        /// <summary>
        /// Enrich the given <paramref name="telemetryEntry"/> with the Operation-related information.
        /// </summary>
        /// <param name="telemetryEntry">The telemetry instance to enrich.</param>
        public void EnrichWithCorrelationInfo<TEntry>(TEntry telemetryEntry) where TEntry : ITelemetry, ISupportProperties
        {
            if (telemetryEntry?.Context?.Operation == null)
            {
                return;
            }

            if (telemetryEntry is RequestTelemetry requestTelemetry)
            {
                if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.OperationId, out string operationId))
                {
                    if (operationId is null || operationId is "null")
                    {
                        operationId = _options.Request.GenerateId();
                    }

                    requestTelemetry.Id = operationId;
                }

                if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.TransactionId, out string transactionId))
                {
                    telemetryEntry.Context.Operation.Id = transactionId;
                }

                if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.OperationParentId, out string operationParentId))
                {
                    telemetryEntry.Context.Operation.ParentId = operationParentId;
                }
            }
            else
            {
                if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.TransactionId, out string transactionId))
                {
                    telemetryEntry.Context.Operation.Id = transactionId;
                }

                if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.OperationId, out string operationId))
                {
                    telemetryEntry.Context.Operation.ParentId = operationId;
                }
            }
        }

        /// <summary>
        /// Enrich the given <paramref name="telemetryEntry"/> with the operation name.
        /// </summary>
        /// <param name="telemetryEntry">The telemetry instance to enrich.</param>
        public void EnrichWithOperationName<TEntry>(TEntry telemetryEntry) where TEntry : ITelemetry, ISupportProperties
        {
            if (telemetryEntry is RequestTelemetry requestTelemetry)
            {
                // Check if operation has already been set with a custom value in the RequestTelemetryConverter, if not use the request name
                if (String.IsNullOrEmpty(requestTelemetry.Context.Operation.Name))
                {
                    requestTelemetry.Context.Operation.Name = requestTelemetry.Name;
                }
            }

            if (telemetryEntry is DependencyTelemetry dependencyTelemetry)
            {
                dependencyTelemetry.Context.Operation.Name = dependencyTelemetry.Name;
            }

            if (telemetryEntry is EventTelemetry eventTelemetry)
            {
                eventTelemetry.Context.Operation.Name = eventTelemetry.Name;
            }

            if (telemetryEntry is AvailabilityTelemetry availabilityTelemetry)
            {
                availabilityTelemetry.Context.Operation.Name = availabilityTelemetry.Name;
            }

            if (telemetryEntry is MetricTelemetry metricTelemetry)
            {
                metricTelemetry.Context.Operation.Name = metricTelemetry.Name;
            }
        }
    }
}
