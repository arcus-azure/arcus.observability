namespace Arcus.Observability.Telemetry.Serilog 
{
    /// <summary>
    /// Represents which application instance the <see cref="ApplicationEnricher"/> should enrich to the log event properties.
    /// </summary>
    public enum ApplicationInstance
    {
        /// <summary>
        /// Uses the environment machine name to add as instance name.
        /// </summary>
        MachineName, 
        
        /// <summary>
        /// Use the Kubernetes pod name environment variable to add as instance name.
        /// </summary>
        PodName
    }
}