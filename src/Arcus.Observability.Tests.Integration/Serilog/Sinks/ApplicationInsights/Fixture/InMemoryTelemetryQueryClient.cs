using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;

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
            if (telemetrySink is null)
            {
                throw new ArgumentNullException(nameof(telemetrySink));
            }

            _telemetrySink = telemetrySink;
        }

        /// <summary>
        /// Gets the tracked traces from the Azure Application Insights instance.
        /// </summary>
        public Task<EventsTraceResult[]> GetTracesAsync()
        {
            return Task.FromResult(_telemetrySink.Traces.Select(t =>
            {
                return new EventsTraceResult(
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
        public Task<EventsMetricsResult[]> GetMetricsAsync(string metricName)
        {
            return Task.FromResult(_telemetrySink.Metrics.Where(m => m.Name == metricName).Select(m => new EventsMetricsResult(m.Name, m.Sum, m.Properties)).ToArray());
        }

        /// <summary>
        /// Gets the tracked custom events from the Azure Application Insights instance.
        /// </summary>
        public Task<EventsCustomEventResult[]> GetCustomEventsAsync()
        {
            return Task.FromResult(_telemetrySink.Events.Select(e => new EventsCustomEventResult(e.Name, e.Context.Cloud.RoleName, e.Properties)).ToArray());
        }

        /// <summary>
        /// Gets the tracked requests from the Azure Application Insights instance.
        /// </summary>
        public Task<EventsRequestResult[]> GetRequestsAsync()
        {
            return Task.FromResult(_telemetrySink.Requests.Select(r =>
            {
                var operation = new OperationResult(r.Context.Operation.Id, r.Context.Operation.ParentId, r.Context.Operation.Name);
                return new EventsRequestResult(
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
        public Task<EventsDependencyResult[]> GetDependenciesAsync()
        {
            return Task.FromResult(_telemetrySink.Dependencies.Select(d =>
            {
                var operation = new OperationResult(d.Context.Operation.Id, d.Context.Operation.ParentId, d.Context.Operation.Name);
                return new EventsDependencyResult(d.Id,
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
        public Task<EventsExceptionResult[]> GetExceptionsAsync()
        {
            return Task.FromResult(_telemetrySink.Exceptions.Select(e =>
            {
                var operation = new OperationResult(e.Context.Operation.Id,
                    e.Context.Operation.ParentId,
                    e.Context.Operation.Name);

                return new EventsExceptionResult(
                    e.Exception.Message,
                    operation,
                    e.Context.Cloud.RoleName,
                    e.Properties);
            }).ToArray());
        }
    }
}