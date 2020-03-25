using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters.Dependencies
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="DependencyTelemetry"/> instance.
    /// </summary>
    public class SqlDependencyTelemetryConverter : DependencyTelemetryConverter
    {
        /// <summary>
        ///     Gets the type of the dependency in an <see cref="DependencyTelemetry"/> instance.
        /// </summary>
        protected override DependencyType DependencyType { get; } = DependencyType.Sql;
    }
}
