using Microsoft.ApplicationInsights.DataContracts;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// The type of the <see cref="DependencyTelemetry"/> instance.
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// Sets the <see cref="DependencyTelemetry"/> as a HTTP dependency.
        /// </summary>
        Http,
        
        /// <summary>
        /// Sets the <see cref="DependencyTelemetry"/> as a SQL dependency.
        /// </summary>
        Sql
    }
}
