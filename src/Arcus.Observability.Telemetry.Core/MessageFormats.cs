namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents the publicly available message formats to track telemetry.
    /// </summary>
    public static class MessageFormats
    {
        /// <summary>
        /// Gets the format to log HTTP requests; compatible with Application Insights 'Requests'.
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
