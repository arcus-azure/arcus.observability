using Microsoft.Extensions.Logging;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// <para>Represents the supported types of telemetry available to track during extensions on the <see cref="ILogger"/>.</para>
    /// <para>Also compatible (but not limited to) with Azure Application Insights (see: <see href="https://observability.arcus-azure.net/features/sinks/azure-application-insights" /> for more information).</para>
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