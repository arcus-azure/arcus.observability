namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents all the log message prefixes that are prepended when an telemetry instance gets logged.
    /// </summary>
    public static class MessagePrefixes
    {
        /// <summary>
        /// Gets the log message prefix when logging Azure Application Insights HTTP dependencies.
        /// </summary>
        public const string DependencyViaHttp = "HTTP Dependency";
        
        /// <summary>
        /// Gets the log message prefix when logging Azure Application Insights SQL dependencies.
        /// </summary>
        public const string DependencyViaSql = "SQL Dependency";
        
        /// <summary>
        /// Gets the log message prefix when logging Azure Application Insights custom dependencies.
        /// </summary>
        public const string Dependency = "Dependency";
        
        /// <summary>
        /// Gets the log message prefix when logging Azure Application Insights Events.
        /// </summary>
        public const string Event = "Events";
        
        /// <summary>
        /// Gets the log message prefix when logging Azure Application Insights Metrics.
        /// </summary>
        public const string Metric = "Metric";
        
        /// <summary>
        /// Gets the log message prefix when logging Azure Application Insights Requests.
        /// </summary>
        public const string RequestViaHttp = "HTTP Request";
    }
}
