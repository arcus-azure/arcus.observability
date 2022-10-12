namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents a way to retrieve the name of an application during the enrichment.
    /// </summary>
    public interface IAppName
    {
        /// <summary>
        /// Represents a way to retrieve the name of an application during the enrichment.
        /// </summary>
        string GetApplicationName();
    }
}
