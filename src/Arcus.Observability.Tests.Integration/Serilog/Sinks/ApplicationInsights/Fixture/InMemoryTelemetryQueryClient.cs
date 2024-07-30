using System.Linq;
using System.Threading.Tasks;
using GuardNet;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture
{
    /// <summary>
    /// Represents an in-memory implementation to query telemetry data from the Azure Application Insights instance.
    /// </summary>
    public class InMemoryTelemetryQueryClient : ITelemetryQueryClient
    {
        private readonly InMemoryApplicationInsightsTelemetryConverter _telemetrySink;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTelemetryQueryClient" /> class.
        /// </summary>
        public InMemoryTelemetryQueryClient(InMemoryApplicationInsightsTelemetryConverter telemetrySink)
        {
            Guard.NotNull(telemetrySink, nameof(telemetrySink));
            _telemetrySink = telemetrySink;
        }

        /// <summary>
        /// Gets the tracked traces from the Azure Application Insights instance.
        /// </summary>
        public Task<TraceResult[]> GetTracesAsync()
        {
            return Task.FromResult(_telemetrySink.Traces.Select(t =>
            {
                return new TraceResult(
                    t.Message,
                    t.Context.Cloud.RoleName,
                    new OperationResult(
                        t.Context.Operation.Id,
                        t.Context.Operation.ParentId,
                        t.Context.Operation.Name),
                    t.Properties);
            }).ToArray());
        }

        /// <summary>
        /// Gets the tracked metrics from the Azure Application Insights instance.
        /// </summary>
        public Task<MetricsResult[]> GetMetricsAsync(string metricName)
        {
            return Task.FromResult(_telemetrySink.Metrics.Where(m => m.Name == metricName).Select(m => new MetricsResult(m.Name, m.Sum, m.Properties)).ToArray());
        }

        /// <summary>
        /// Gets the tracked custom events from the Azure Application Insights instance.
        /// </summary>
        public Task<CustomEventResult[]> GetCustomEventsAsync()
        {
            return Task.FromResult(_telemetrySink.Events.Select(e => new CustomEventResult(e.Name, e.Context.Cloud.RoleName, e.Properties)).ToArray());
        }

        /// <summary>
        /// Gets the tracked requests from the Azure Application Insights instance.
        /// </summary>
        public Task<RequestResult[]> GetRequestsAsync()
        {
            return Task.FromResult(_telemetrySink.Requests.Select(r =>
            {
                var operation = new OperationResult(r.Context.Operation.Id, r.Context.Operation.ParentId, r.Context.Operation.Name);
                return new RequestResult(
                    r.Id,
                    r.Name,
                    r.Source,
                    r.Url?.ToString(),
                    r.Success ?? false,
                    r.ResponseCode,
                    r.Context.Cloud.RoleName,
                    operation,
                    r.Properties);
            }).ToArray());
        }

        /// <summary>
        /// Gets the tracked dependencies from the Azure Application Insights instance.
        /// </summary>
        public Task<DependencyResult[]> GetDependenciesAsync()
        {
            return Task.FromResult(_telemetrySink.Dependencies.Select(d =>
            {
                var operation = new OperationResult(d.Context.Operation.Id, d.Context.Operation.ParentId, d.Context.Operation.Name);
                return new DependencyResult(d.Id,
                    d.Type,
                    d.Target,
                    d.Data,
                    d.Success ?? false,
                    string.IsNullOrWhiteSpace(d.ResultCode) ? 0 : int.Parse(d.ResultCode),
                    d.Name,
                    d.Context.Cloud.RoleName,
                    operation,
                    d.Properties);
            }).ToArray());
        }

        /// <summary>
        /// Gets the tracked exceptions from the Azure Application Insights instance.
        /// </summary>
        public Task<ExceptionResult[]> GetExceptionsAsync()
        {
            return Task.FromResult(_telemetrySink.Exceptions.Select(e =>
            {
                var operation = new OperationResult(e.Context.Operation.Id,
                    e.Context.Operation.ParentId,
                    e.Context.Operation.Name);

                return new ExceptionResult(
                    e.Exception.Message,
                    operation,
                    e.Context.Cloud.RoleName,
                    e.Properties);
            }).ToArray());
        }
    }
}