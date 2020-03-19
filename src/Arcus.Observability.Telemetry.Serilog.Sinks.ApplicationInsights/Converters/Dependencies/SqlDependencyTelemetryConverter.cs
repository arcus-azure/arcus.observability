namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters.Dependencies
{
    public class SqlDependencyTelemetryConverter : DependencyTelemetryConverter
    {
        /// <inheritdoc />
        protected override DependencyType DependencyType { get; } = DependencyType.Sql;
    }
}
