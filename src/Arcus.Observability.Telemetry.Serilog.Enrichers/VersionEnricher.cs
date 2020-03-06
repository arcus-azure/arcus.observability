using System;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enrichment on log events with the current runtime version (i.e. 'version' = '1.0.0').
    /// </summary>
    public class VersionEnricher : ILogEventEnricher
    {
        private readonly string _assemblyVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionEnricher"/> class.
        /// </summary>
        public VersionEnricher()
        {
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
                var versionProperty = propertyFactory.CreateProperty("version", _assemblyVersion);
                logEvent.AddPropertyIfAbsent(versionProperty);
            }
        }
    }
}
