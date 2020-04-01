using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

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

            if (telemetryEntry.Properties.TryGetValue(ContextProperties.Correlation.OperationId, out string correlationId))
            {
                telemetryEntry.Context.Operation.Id = correlationId;
            }
        }
    }
}
