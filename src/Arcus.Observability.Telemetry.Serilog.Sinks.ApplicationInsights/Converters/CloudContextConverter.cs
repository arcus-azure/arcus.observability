using System;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from the Cloud-related logging information to the Application Insights <see cref="CloudContext"/> instance.
    /// </summary>
    public class CloudContextConverter
    {
        /// <summary>
        /// Enrich the given <paramref name="telemetry"/> with the Cloud-related information found in the <paramref name="logEvent"/>.
        /// </summary>
        /// <param name="logEvent">The log event that may contains Cloud-related information.</param>
        /// <param name="telemetry">The telemetry instance to enrich.</param>
        public void EnrichWithAppInfo(LogEvent logEvent, ITelemetry telemetry)
        {
            if (telemetry.Context?.Cloud == null)
            {
                return;
            }

            var componentName = logEvent.Properties.GetAsRawString(ContextProperties.General.ComponentName);
            var machineName = logEvent.Properties.GetAsRawString(ContextProperties.General.MachineName);
            var podName = logEvent.Properties.GetAsRawString(ContextProperties.Kubernetes.PodName);

            telemetry.Context.Cloud.RoleName = componentName;
            telemetry.Context.Cloud.RoleInstance = string.IsNullOrWhiteSpace(podName) ? machineName : podName;

            //if (telemetry is RequestTelemetry f)
            //{
            //    f.Context.Operation.Name = f.Name;
            //}

            //if (telemetry is DependencyTelemetry d)
            //{
            //    d.Context.Operation.Name = d.Name;
            //}
        }
    }
}
