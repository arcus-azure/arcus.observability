using System;
using System.Reflection;
using GuardNet;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents an <see cref="IAppVersion"/> implementation that uses the assembly version as application version.
    /// </summary>
    /// <seealso cref="IAppVersion"/>
#pragma warning disable S1133
    [Obsolete("Will be removed in v4.0 as application versioning enrichment is too project-specific")]
#pragma warning restore S1133
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

            _assemblyVersion = new Lazy<string>(() => GetAssemblyVersion(executingAssembly));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAppVersion"/> class.
        /// </summary>
        /// <param name="consumerType">Some random consumer type to have access to the assembly of the executing project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="consumerType"/> is <c>null</c>.</exception>
        public AssemblyAppVersion(Type consumerType)
        {
            Guard.NotNull(consumerType, nameof(consumerType), "Requires a consumer type to retrieve the assembly where the project runs");

            Assembly executingAssembly = consumerType.Assembly;
            _assemblyVersion = new Lazy<string>(() => GetAssemblyVersion(executingAssembly));
        }

        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        public string GetVersion()
        {
            return _assemblyVersion.Value;
        }

        private static string GetAssemblyVersion(Assembly executingAssembly)
        {
            return executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? executingAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
                   ?? executingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        }
    }
}
