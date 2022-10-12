using System;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration
{
    /// <summary>
    /// Represents an <see cref="ITelemetryInitializer"/> that configures the application information.
    /// </summary>
    public class ApplicationTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IAppName _applicationName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTelemetryInitializer" /> class.
        /// </summary>
        /// <param name="applicationName">The instance to retrieve the application name.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="applicationName"/> is <c>null</c>.</exception>
        public ApplicationTelemetryInitializer(IAppName applicationName)
        {
            Guard.NotNull(applicationName, nameof(applicationName), $"Requires an application name ({nameof(IAppName)}) implementation to retrieve the application name to initialize in the telemetry");
            _applicationName = applicationName;
        }

        /// <summary>
        /// Initializes properties of the specified <see cref="T:Microsoft.ApplicationInsights.Channel.ITelemetry" /> object.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry?.Context?.Cloud != null)
            {
                string componentName = _applicationName.GetApplicationName();
                telemetry.Context.Cloud.RoleName = componentName; 
            }
        }
    }
}
