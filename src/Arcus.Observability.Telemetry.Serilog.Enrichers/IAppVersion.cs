namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Represents a way to retrieve the version of an application during the enrichment in the <see cref="VersionEnricher"/>.
    /// </summary>
    public interface IAppVersion
    {
        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        string GetVersion();
    }
}
