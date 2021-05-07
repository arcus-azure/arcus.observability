using Microsoft.Extensions.Logging;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents the supported types of telemetry available to track during extensions on the <see cref="ILogger"/>.
    /// Compatible with Azure Application Insights.
    /// </summary>
    public enum TelemetryType
    {
        /// <summary>
        /// Specifies the type of logged telemetry as traces.
        /// </summary>
        Trace = 0,
        
        /// <summary>
        /// Specifies the type of logged telemetry as tracked dependencies.
        /// </summary>
        Dependency = 1,
        
        /// <summary>
        /// Specifies the type of logged telemetry as HTTP requests.
        /// </summary>
        Request = 2,
        
        /// <summary>
        /// Specifies the type of logged telemetry as custom events.
        /// </summary>
        Events = 4,
        
        /// <summary>
        /// Specifies the type of logged telemetry as value metrics.
        /// </summary>
        Metrics = 8
    }
}