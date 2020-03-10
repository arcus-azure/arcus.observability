using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public class CloudContextConverter
    {
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
        }
    }
}
