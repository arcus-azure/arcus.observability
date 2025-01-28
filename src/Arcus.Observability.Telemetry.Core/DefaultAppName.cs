using System;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Default <see cref="IAppName"/> implementation that uses a static name to set as application name.
    /// </summary>
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
            if (string.IsNullOrWhiteSpace(componentName))
            {
                throw new ArgumentNullException(nameof(componentName), "Requires a non-blank functional name to identify the application");
            }

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
