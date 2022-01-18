namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration
{
    /// <summary>
    /// User-defined configuration options to influence the behavior of the Azure Application Insights Serilog sink.
    /// </summary>
    public class ApplicationInsightsSinkOptions
    {
        /// <summary>
        /// Gets the Application Insights options related to tracking requests.
        /// </summary>
        public ApplicationInsightsSinkRequestOptions Request { get; } = new ApplicationInsightsSinkRequestOptions();
        
        /// <summary>
        /// Gets the Application Insights options related to tracking exceptions.
        /// </summary>
        public ApplicationInsightsSinkExceptionOptions Exception { get; } = new ApplicationInsightsSinkExceptionOptions();
    }
}
