using System;
using System.Reflection;
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

        private readonly string _propertyName, _assemblyVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class.
        /// </summary>
        public VersionEnricher() : this(DefaultPropertyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to enrich the log event with the current runtime version.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyName"/> is blank.</exception>
        public VersionEnricher(string propertyName)
        {
            Guard.NotNullOrWhitespace(propertyName, nameof(propertyName), "Requires a non-blank property name to enrich the log event with the current runtime version");
            
            _propertyName = propertyName;

            var executingAssembly = Assembly.GetEntryAssembly();
            if (executingAssembly == null)
            {
                throw new InvalidOperationException(
                    "Cannot enrich the log events with a 'Version' because the version of the current executing runtime couldn't be determined");
            }
            
            _assemblyVersion = 
                executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? executingAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
                    ?? executingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!String.IsNullOrWhiteSpace(_assemblyVersion))
            {
                var versionProperty = propertyFactory.CreateProperty(_propertyName, _assemblyVersion);
                logEvent.AddPropertyIfAbsent(versionProperty);
            }
        }
    }
}
