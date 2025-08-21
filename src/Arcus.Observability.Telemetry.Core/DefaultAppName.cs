using System;
using GuardNet;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Default <see cref="IAppName"/> implementation that uses a static name to set as application name.
    /// </summary>
#pragma warning disable S1133
    [Obsolete("Will be removed in v4.0 as application name enrichment is too project specific")]
#pragma warning restore S1133
    public class DefaultAppName : IAppName
    {
        private readonly string _componentName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAppName" /> class.
        /// </summary>
        /// <param name="componentName">The functional name to identity the application.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="componentName"/> is blank.</exception>
        public DefaultAppName(string componentName)
        {
            Guard.NotNullOrWhitespace(componentName, nameof(componentName), "Requires a non-blank functional name to identity the application");
            _componentName = componentName;
        }

        /// <summary>
        /// Represents a way to retrieve the name of an application during the enrichment.
        /// </summary>
        public string GetApplicationName()
        {
            return _componentName;
        }
    }
}
