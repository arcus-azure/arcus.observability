namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    ///     Different types of telemetry
    /// </summary>
    public enum TelemetryType
    {
        Trace,
        Dependency,
        Request,
        Events,
        Metrics
    }
}