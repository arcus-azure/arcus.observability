﻿using System;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration
{
    /// <summary>
    /// Represents an <see cref="ITelemetryInitializer"/> that configures the application's version.
    /// </summary>
    public class ApplicationVersionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IAppVersion _applicationVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationVersionTelemetryInitializer" /> class.
        /// </summary>
        /// <param name="applicationVersion">The instance to retrieve the application version.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="applicationVersion"/> is <c>null</c>.</exception>
        public ApplicationVersionTelemetryInitializer(IAppVersion applicationVersion)
        {
            Guard.NotNull(applicationVersion, nameof(applicationVersion), $"Requires an application version ({nameof(IAppVersion)}) implementation to retrieve the application version to initialize in the telemetry");
            _applicationVersion = applicationVersion;
        }

        /// <summary>
        /// Initializes properties of the specified <see cref="T:Microsoft.ApplicationInsights.Channel.ITelemetry" /> object.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry?.Context?.Component != null)
            {
                string version = _applicationVersion.GetVersion();
                telemetry.Context.Component.Version = version;
            }
        }
    }
}
