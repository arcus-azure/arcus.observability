using System;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enrichment on log events with the current runtime version (i.e. 'version' = '1.0.0').
    /// </summary>
    public class VersionEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Gets the default property name that the <see cref="VersionEnricher"/> will use when the version gets enriched on the log event.
        /// </summary>
        public const string DefaultPropertyName = "version";

        private readonly IAppVersion _appVersion;
        private readonly string _propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class that uses the assembly version as application version.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the process executable in the default application domain cannot be retrieved.</exception>
        public VersionEnricher() : this(new AssemblyAppVersion(), DefaultPropertyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to enrich the log event with the current runtime version.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyName"/> is blank.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the process executable in the default application domain cannot be retrieved.</exception>
        public VersionEnricher(string propertyName) : this(new AssemblyAppVersion(), propertyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class.
        /// </summary>
        /// <param name="appVersion">The instance to retrieve the current application version.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="appVersion"/> is <c>null</c>.</exception>
        public VersionEnricher(IAppVersion appVersion) : this(appVersion, DefaultPropertyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class.
        /// </summary>
        /// <param name="appVersion">The instance to retrieve the current application version.</param>
        /// <param name="propertyName">The name of the property to enrich the log event with the current runtime version.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="appVersion"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyName"/> is blank.</exception>
        public VersionEnricher(IAppVersion appVersion, string propertyName)
        {
            Guard.NotNull(appVersion, nameof(appVersion), "Requires an application version implementation to enrich the log event with the application version");
            Guard.NotNullOrWhitespace(propertyName, nameof(propertyName), "Requires a non-blank property name to enrich the log event with the current runtime version");

            _appVersion = appVersion;
            _propertyName = propertyName;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string version = _appVersion.GetVersion();
            if (!String.IsNullOrWhiteSpace(version))
            {
                var versionProperty = propertyFactory.CreateProperty(_propertyName, version);
                logEvent.AddPropertyIfAbsent(versionProperty);
            }
        }
    }
}
