using System;
using System.Reflection;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Represents an <see cref="IAppVersion"/> implementation that uses the assembly version as application version.
    /// </summary>
    /// <seealso cref="IAppVersion"/>
    public class AssemblyAppVersion : IAppVersion
    {
        private readonly Lazy<string> _assemblyVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAppVersion"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the process executable in the default application domain cannot be retrieved.</exception>
        public AssemblyAppVersion()
        {
            var executingAssembly = Assembly.GetEntryAssembly();
            if (executingAssembly is null)
            {
                throw new InvalidOperationException(
                    "Cannot enrich the log events with a 'Version' because the version of the current executing runtime couldn't be determined");
            }
            
            _assemblyVersion = new Lazy<string>(() => 
                executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? executingAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
                    ?? executingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version);
        }

        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        public string GetVersion()
        {
            return _assemblyVersion.Value;
        }
    }
}
