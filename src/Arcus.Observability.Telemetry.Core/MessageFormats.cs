using System;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents the publicly available message formats to track telemetry.
    /// </summary>
    public static class MessageFormats
    {
        /// <summary>
        /// Gets the message format to log external dependencies; compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string DependencyFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} {"
            + ContextProperties.DependencyTracking.DependencyData + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        /// <summary>
        /// Gets the message format to log external dependencies without any custom data value (<see cref="ContextProperties.DependencyTracking.DependencyData"/>);
        /// compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string DependencyWithoutDataFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        /// <summary>
        /// Gets the message format to log Azure Service Bus dependencies; compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string ServiceBusDependencyFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} {"
            + ContextProperties.DependencyTracking.ServiceBus.EntityType + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        /// <summary>
        /// Gets the message format to log HTTP dependencies; compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string HttpDependencyFormat =
            MessagePrefixes.DependencyViaHttp + " {"
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName + "} completed with {"
            + ContextProperties.DependencyTracking.ResultCode + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        /// <summary>
        /// Gets the message format to log SQL dependencies; compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string SqlDependencyFormat =
            MessagePrefixes.DependencyViaSql + " {" 
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName
            + "} for operation {" + ContextProperties.DependencyTracking.DependencyData
            + "} in {" + ContextProperties.DependencyTracking.Duration
            + "} at {" + ContextProperties.DependencyTracking.StartTime
            + "} (Successful: {" + ContextProperties.DependencyTracking.IsSuccessful
            + "} - Context: {@" + ContextProperties.TelemetryContext + "})";

        /// <summary>
        /// Gets the message format to log events; compatible with Application Insights 'Events'.
        /// </summary>
        [Obsolete("Use the " + nameof(EventFormat) + " instead")]
        public const string OldEventFormat = 
            MessagePrefixes.Event + " {" 
            + ContextProperties.EventTracking.EventName
#pragma warning disable 618 // Use 'ContextProperties.TelemetryContext' once we remove 'EventDescription'.
            + "} (Context: {@" + ContextProperties.EventTracking.EventContext + "})";
#pragma warning restore 618

        /// <summary>
        /// Gets the message format to log events; compatible with Application Insights 'Events'.
        /// </summary>
        public const string EventFormat = 
            MessagePrefixes.Event + " {" 
            + ContextProperties.EventTracking.EventName
            + "} (Context: {@" + ContextProperties.TelemetryContext + "})";
        
        /// <summary>
        /// Gets the message format to log metrics; compatible with Application Insights 'Metrics'.
        /// </summary>
        public const string MetricFormat =
            MessagePrefixes.Metric + " {" 
            + ContextProperties.MetricTracking.MetricName + "}: {" 
            + ContextProperties.MetricTracking.MetricValue + "} at {"
            + ContextProperties.MetricTracking.Timestamp
            + "} (Context: {@" + ContextProperties.TelemetryContext + "})";
        
        /// <summary>
        /// Gets the message format to log HTTP requests; compatible with Application Insights 'Requests'.
        /// </summary>
        public const string RequestFormat =
            MessagePrefixes.RequestViaHttp + " {"
            + ContextProperties.RequestTracking.RequestMethod + "} {"
            + ContextProperties.RequestTracking.RequestHost + "}/{" 
            + ContextProperties.RequestTracking.RequestUri + "} completed with {"
            + ContextProperties.RequestTracking.ResponseStatusCode + "} in {"
            + ContextProperties.RequestTracking.RequestDuration + "} at {"
            + ContextProperties.RequestTracking.RequestTime + "} - (Context: {@"
            + ContextProperties.TelemetryContext + "})";
    }
}
