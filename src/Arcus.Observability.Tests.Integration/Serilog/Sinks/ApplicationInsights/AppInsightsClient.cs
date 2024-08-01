using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Integration.Configuration;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Arcus.Testing;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Newtonsoft.Json;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Represents a remote client to query telemetry data from the Azure Application Insights instance.
    /// </summary>
    public class AppInsightsClient : ITelemetryQueryClient
    {
        private readonly LogsQueryClient _queryClient;
        private readonly QueryTimeRange _timeRange;
        private readonly string _workspaceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsClient" /> class.
        /// </summary>
        public AppInsightsClient(TestConfig config)
        {
            ServicePrincipal servicePrincipal = config.GetServicePrincipal();
            _queryClient = new LogsQueryClient(new ClientSecretCredential(servicePrincipal.TenantId, servicePrincipal.ClientId, servicePrincipal.ClientSecret));
            _workspaceId = config["Arcus:ApplicationInsights:LogAnalytics:WorkspaceId"];
            _timeRange = new QueryTimeRange(TimeSpan.FromDays(1));
        }

        /// <summary>
        /// Gets the tracked traces from the Azure Application Insights instance.
        /// </summary>
        public async Task<TraceResult[]> GetTracesAsync()
        {
            IReadOnlyCollection<LogsTableRow> rows = 
                await QueryLogsAsync("AppTraces | project Message, OperationId, ParentId, AppRoleName, Properties");

            return rows.Select(row =>
            {
                string message = row[0].ToString();
                string operationId = row[1].ToString();
                string parentId = row[2].ToString();
                var operation = new OperationResult(operationId, parentId);

                string roleName = row[3].ToString();
                string customDimensionsTxt = row[4].ToString();
                var customDimensions = JsonConvert.DeserializeObject<Dictionary<string, string>>(customDimensionsTxt);

                return new TraceResult(message, roleName, operation, customDimensions);

            }).ToArray();
        }

        /// <summary>
        /// Gets the tracked metrics from the Azure Application Insights instance.
        /// </summary>
        public Task<MetricsResult[]> GetMetricsAsync(string metricName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the tracked custom events from the Azure Application Insights instance.
        /// </summary>
        public async Task<CustomEventResult[]> GetCustomEventsAsync()
        {
            IReadOnlyCollection<LogsTableRow> rows = 
                await QueryLogsAsync("AppEvents | project Name, AppRoleName, Properties");

            return rows.Select(row =>
            {
                string name = row[0].ToString();
                string roleName = row[1].ToString();

                var customDimensionsTxt = row[2].ToString();
                var customDimensions = JsonConvert.DeserializeObject<Dictionary<string, string>>(customDimensionsTxt);

                return new CustomEventResult(name, roleName, customDimensions);

            }).ToArray();
        }

        /// <summary>
        /// Gets the tracked requests from the Azure Application Insights instance.
        /// </summary>
        public async Task<RequestResult[]> GetRequestsAsync()
        {
            IReadOnlyCollection<LogsTableRow> rows =
                await QueryLogsAsync(
                    "AppRequests | project Id, Name, Source, Url, Success, ResultCode, AppRoleName, OperationId, ParentId, Properties");
            
            return rows.Select(row =>
            {
                string id = row[0].ToString();
                string name = row[1].ToString();
                string source = row[2].ToString();
                string url = row[3].ToString();
                bool success = bool.Parse(row[4].ToString() ?? string.Empty);
                string resultCode = row[5].ToString();
                string roleName = row[6].ToString();

                string operationId = row[7].ToString();
                string operationParentId = row[8].ToString();
                var operation = new OperationResult(operationId, operationParentId, name);

                var customDimensionsTxt = row[9].ToString();
                var customDimensions = JsonConvert.DeserializeObject<Dictionary<string, string>>(customDimensionsTxt);

                return new RequestResult(id, name, source, url, success, resultCode, roleName, operation, customDimensions);
                
            }).ToArray();
        }

        /// <summary>
        /// Gets the tracked dependencies from the Azure Application Insights instance.
        /// </summary>
        public async Task<DependencyResult[]> GetDependenciesAsync()
        {
            IReadOnlyCollection<LogsTableRow> rows = 
                await QueryLogsAsync("AppDependencies | project Id, Target, DependencyType, Name, Data, Success, ResultCode, AppRoleName, OperationId, ParentId, Properties");

            return rows.Select(row =>
            {
                string id = row[0].ToString();
                string target = row[1].ToString();
                string type = row[2].ToString();
                string name = row[3].ToString();
                string data = row[4].ToString();

                bool success = bool.Parse(row[5].ToString() ?? string.Empty);
                var resultCodeTxt = row[6].ToString();
                int resultCode = string.IsNullOrWhiteSpace(resultCodeTxt) ? 0 : int.Parse(resultCodeTxt);

                string roleName = row[7].ToString();

                string operationId = row[8].ToString();
                string operationParentId = row[9].ToString();
                var operation = new OperationResult(operationId, operationParentId);

                var customDimensionsTxt = row[10].ToString();
                var customDimensions = JsonConvert.DeserializeObject<Dictionary<string, string>>(customDimensionsTxt);

                return new DependencyResult(id, type, target, data, success, resultCode, name, roleName, operation, customDimensions);

            }).ToArray();
        }

        /// <summary>
        /// Gets the tracked exceptions from the Azure Application Insights instance.
        /// </summary>
        public async Task<ExceptionResult[]> GetExceptionsAsync()
        {
            IReadOnlyCollection<LogsTableRow> rows = 
                await QueryLogsAsync("AppExceptions | project OuterMessage, OperationId, ParentId, AppRoleName, Properties");

            return rows.Select(row =>
            {
                string message = row[0].ToString();
                string operationId = row[1].ToString();
                string parentId = row[2].ToString();
                var operation = new OperationResult(operationId, parentId);

                string roleName = row[3].ToString();

                string customDimensionsTxt = row[4].ToString();
                var customDimensions = JsonConvert.DeserializeObject<Dictionary<string, string>>(customDimensionsTxt);

                return new ExceptionResult(message, operation, roleName, customDimensions);

            }).ToArray();
        }

        private async Task<IReadOnlyCollection<LogsTableRow>> QueryLogsAsync(string query)
        {
            LogsQueryResult response = await _queryClient.QueryWorkspaceAsync(
                _workspaceId,
                query,
                timeRange: _timeRange,
                new LogsQueryOptions { ServerTimeout = TimeSpan.FromSeconds(3) });
            
            return response.Table.Rows;
        }
    }
}
