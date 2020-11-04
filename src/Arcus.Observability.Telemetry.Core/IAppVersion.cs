namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents a way to retrieve the version of an application during the enrichment.
    /// </summary>
    public interface IAppVersion
    {
        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        string GetVersion();
    }
}
