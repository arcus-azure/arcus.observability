namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters.Dependencies
{
    public class HttpDependencyTelemetryConverter : DependencyTelemetryConverter
    {
        /// <inheritdoc />
        protected override DependencyType DependencyType { get; } = DependencyType.Http;
    }
}
