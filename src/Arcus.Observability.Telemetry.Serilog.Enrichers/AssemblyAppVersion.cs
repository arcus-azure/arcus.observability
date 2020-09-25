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
        private readonly Assembly _executingAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAppVersion"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the process executable in the default application domain cannot be retrieved.</exception>
        public AssemblyAppVersion()
        {
            var executingAssembly = Assembly.GetEntryAssembly();
            if (executingAssembly == null)
            {
                throw new InvalidOperationException(
                    "Cannot enrich the log events with a 'Version' because the version of the current executing runtime couldn't be determined");
            }

            _executingAssembly = executingAssembly;
        }

        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        public string GetVersion()
        {
            return _executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? _executingAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
                ?? _executingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        }
    }
}
