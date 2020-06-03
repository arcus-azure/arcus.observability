namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Represents from where the cloud context role instance should be retrieved.
    /// </summary>
    public enum RoleInstance
    {
        /// <summary>
        /// Use the current environment name as the cloud context role instance.
        /// </summary>
        MachineName = 0,
        
        /// <summary>
        /// Use the Kubernetes information's pod name as cloud context role instance.
        /// </summary>
        PodName = 1
    }
}
