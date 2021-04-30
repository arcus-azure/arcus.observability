using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters.Dependencies
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="DependencyTelemetry"/> instance.
    /// </summary>
    public class HttpDependencyTelemetryConverter : DependencyTelemetryConverter
    {
        /// <summary>
        ///     Gets the custom dependency type name from the given <paramref name="logEntry"/> to use in an <see cref="DependencyTelemetry"/> instance.
        /// </summary>
        /// <param name="logEntry">The logged event.</param>
        protected override string GetDependencyType(StructureValue logEntry)
        {
            return DependencyType.Http.ToString();
        }
    }
}
