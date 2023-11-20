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
