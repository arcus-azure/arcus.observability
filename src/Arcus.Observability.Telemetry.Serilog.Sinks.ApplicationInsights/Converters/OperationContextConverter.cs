using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from the Operation-related logging information to the Application Insights <see cref="OperationContext"/> instance.
    /// </summary>
    public class OperationContextConverter
    {
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

            if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.OperationId, out string operationId))
            {
                telemetryEntry.Context.Operation.Id = operationId;
            }

            if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.OperationParentId, out string operationParentId))
            {
                telemetryEntry.Context.Operation.ParentId = operationParentId;
            }
        }

        /// <summary>
        /// Enrich the given <paramref name="telemetryEntry"/> with the operation name.
        /// </summary>
        /// <param name="telemetryEntry">The telemetry instance to enrich.</param>
        public void EnrichWithOperationName<TEntry>(TEntry telemetryEntry) where TEntry : ITelemetry, ISupportProperties
        {
            if (telemetryEntry is RequestTelemetry r)
            {
                // Check if operation has already been set with a custom value in the RequestTelemetryConverter, if not use the request name
                if (String.IsNullOrEmpty(r.Context.Operation.Name))
                {
                    r.Context.Operation.Name = r.Name;
                }
            }

            if (telemetryEntry is DependencyTelemetry d)
            {
                d.Context.Operation.Name = d.Name;
            }

            if (telemetryEntry is EventTelemetry e)
            {
                e.Context.Operation.Name = e.Name;
            }

            if (telemetryEntry is AvailabilityTelemetry a)
            {
                a.Context.Operation.Name = a.Name;
            }

            if (telemetryEntry is MetricTelemetry m)
            {
                m.Context.Operation.Name = m.Name;
            }
        }
    }
}
