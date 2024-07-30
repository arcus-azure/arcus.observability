using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture
{
    /// <summary>
    /// Represents a client to query telemetry data from the Azure Application Insights instance.
    /// </summary>
    public interface ITelemetryQueryClient
    {
        /// <summary>
        /// Gets the tracked traces from the Azure Application Insights instance.
        /// </summary>
        Task<TraceResult[]> GetTracesAsync();

        /// <summary>
        /// Gets the tracked metrics from the Azure Application Insights instance.
        /// </summary>
        Task<MetricsResult[]> GetMetricsAsync(string metricName);
        
        /// <summary>
        /// Gets the tracked custom events from the Azure Application Insights instance.
        /// </summary>
        Task<CustomEventResult[]> GetCustomEventsAsync();

        /// <summary>
        /// Gets the tracked requests from the Azure Application Insights instance.
        /// </summary>
        Task<RequestResult[]> GetRequestsAsync();

        /// <summary>
        /// Gets the tracked dependencies from the Azure Application Insights instance.
        /// </summary>
        Task<DependencyResult[]> GetDependenciesAsync();

        /// <summary>
        /// Gets the tracked exceptions from the Azure Application Insights instance.
        /// </summary>
        Task<ExceptionResult[]> GetExceptionsAsync();
    }

     public class OperationResult
    {
        public OperationResult(string id, string parentId)
        {
            Id = id;
            ParentId = parentId;
        }

        public OperationResult(string id, string parentId, string name)
        {
            Id = id;
            ParentId = parentId;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
        public string ParentId { get; }
    }

    public class TraceResult
    {
        public TraceResult(string message, string roleName, OperationResult operation, IDictionary<string, string> customDimensions)
        {
            Message = message;
            RoleName = roleName;
            Operation = operation;
            CustomDimensions = customDimensions;
        }

        public string Message { get; }
        public string RoleName { get; }
        public OperationResult Operation { get; }
        public IDictionary<string, string> CustomDimensions { get; }
    }

    public class CustomEventResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEventResult" /> class.
        /// </summary>
        public CustomEventResult(string name, string roleName, IDictionary<string, string> customDimensions)
        {
            Name = name;
            RoleName = roleName;
            CustomDimensions = new ReadOnlyDictionary<string, string>(customDimensions);
        }

        public string Name { get; }
        public string RoleName { get; }
        public IReadOnlyDictionary<string, string> CustomDimensions { get; }
    }

    public class MetricsResult
    {
        public MetricsResult(string name, double value, IDictionary<string, string> customDimensions)
        {
            Name = name;
            Value = value;
            CustomDimensions = customDimensions;
        }

        public string Name { get; }
        public double Value { get; }
        public IDictionary<string, string> CustomDimensions { get; }
    }

    public class RequestResult
    {
        public RequestResult(
            string id,
            string name,
            string source,
            string url,
            bool success,
            string resultCode,
            string roleName,
            OperationResult operation,
            IDictionary<string, string> customDimensions)
        {
            Id = id;
            Name = name;
            Source = source;
            Url = url;
            Success = success;
            ResultCode = resultCode;
            RoleName = roleName;
            Operation = operation;
            CustomDimensions = customDimensions;
        }

        public string Id { get; }
        public string Name { get; }
        public string Source { get; }
        public string Url { get; }
        public bool Success { get; }
        public string ResultCode { get; }
        public string RoleName { get; }
        public OperationResult Operation { get; }
        public IDictionary<string, string> CustomDimensions { get; }
    }

    public class DependencyResult
    {
        public DependencyResult(
            string id,
            string type,
            string target,
            string data,
            bool success,
            int resultCode,
            string name,
            string roleName,
            OperationResult operation,
            IDictionary<string, string> customDimensions)
        {
            Id = id;
            Type = type;
            Target = target;
            Data = data;
            Success = success;
            ResultCode = resultCode;
            Name = name;
            RoleName = roleName;
            Operation = operation;
            CustomDimensions = customDimensions;
        }

        public string Type { get; }
        public string Id { get; }
        public string Target { get; }
        public string Data { get; }
        public bool Success { get; }
        public int ResultCode { get; }
        public string Name { get; }
        public string RoleName { get; }
        public OperationResult Operation { get; }
        public IDictionary<string, string> CustomDimensions { get; }
    }

    public class ExceptionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionResult" /> class.
        /// </summary>
        public ExceptionResult(string message, OperationResult operation, string roleName, IDictionary<string, string> customDimensions)
        {
            Message = message;
            Operation = operation;
            RoleName = roleName;
            CustomDimensions = customDimensions;
        }

        public string Message { get; }
        public OperationResult Operation { get; }
        public string RoleName { get; }
        public IDictionary<string, string> CustomDimensions { get; }

    }
}