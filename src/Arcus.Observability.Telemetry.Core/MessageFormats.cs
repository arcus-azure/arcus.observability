using System;
using Arcus.Observability.Telemetry.Core.Logging;

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
        public const string DependencyFormat = "{@" + ContextProperties.DependencyTracking.DependencyLogEntry + "}";

        /// <summary>
        /// Gets the message format to log external dependencies without any custom data value (<see cref="ContextProperties.DependencyTracking.DependencyData"/>);
        /// compatible with Application Insights 'Dependencies'.
        /// </summary>
        [Obsolete("Dependencies without data will be logged as '" + nameof(DependencyLogEntry) + "' models instead of this sentence-like format")]
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
        [Obsolete("Dependencies without data will be logged as '" + nameof(DependencyLogEntry) + "' models instead of this sentence-like format")]
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
        public const string HttpDependencyFormat = "{@" + ContextProperties.DependencyTracking.DependencyLogEntry + "}";

        /// <summary>
        /// Gets the message format to log SQL dependencies; compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string SqlDependencyFormat = "{@" + ContextProperties.DependencyTracking.DependencyLogEntry + "}";

        /// <summary>
        /// Gets the message format to log events; compatible with Application Insights 'Events'.
        /// </summary>
        public const string EventFormat = "{@" + ContextProperties.EventTracking.EventLogEntry + "}";

        /// <summary>
        /// Gets the message format to log metrics; compatible with Application Insights 'Metrics'.
        /// </summary>
        public const string MetricFormat = "{@" + ContextProperties.MetricTracking.MetricLogEntry + "}";
        
        /// <summary>
        /// Gets the message format to log HTTP requests; compatible with Application Insights 'Requests'.
        /// </summary>
        public const string RequestFormat = "{@" + ContextProperties.RequestTracking.RequestLogEntry + "}";
    }
}
