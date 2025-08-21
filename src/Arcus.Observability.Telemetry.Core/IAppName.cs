using System;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents a way to retrieve the name of an application during the enrichment.
    /// </summary>
#pragma warning disable S1133
    [Obsolete("Will be removed in v4.0 as application name enrichment is too project-specific")]
#pragma warning restore S1133
    public interface IAppName
    {
        /// <summary>
        /// Represents a way to retrieve the name of an application during the enrichment.
        /// </summary>
        string GetApplicationName();
    }
}
