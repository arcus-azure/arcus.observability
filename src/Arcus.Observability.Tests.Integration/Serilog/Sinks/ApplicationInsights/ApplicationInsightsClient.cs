using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuardNet;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Xunit;
using Xunit.Sdk;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Represents a simple client to interact with the available features of Application Insights.
    /// </summary>
    public class ApplicationInsightsClient
    {
        private const string PastHalfHourTimeSpan = "PT30M";

        private readonly IApplicationInsightsDataClient _dataClient;
        private readonly string _applicationId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsClient" /> class.
        /// </summary>
        /// <param name="dataClient">The core client to retrieve telemetry from Application Insights.</param>
        /// <param name="applicationId">The application ID to identify the Application Insights resource to request telemetry from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="dataClient"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="applicationId"/> is blank.</exception>
        public ApplicationInsightsClient(IApplicationInsightsDataClient dataClient, string applicationId)
        {
            Guard.NotNull(dataClient, nameof(dataClient), "Requires a data client implementation to retrieve telemetry form Application Insights");
            Guard.NotNullOrWhitespace(applicationId, nameof(applicationId), "Requires an application ID to identify the Application Insights resource to request telemetry from");
            
            _dataClient = dataClient;
            _applicationId = applicationId;
        }

        /// <summary>
        /// Gets the currently available dependencies tracked on Application Insights.
        /// </summary>
        /// <exception cref="NotEmptyException">Thrown when the retrieved telemetry doesn't contain any items.</exception>
        public async Task<EventsDependencyResult[]> GetDependenciesAsync()
        {
            EventsResults<EventsDependencyResult> result = 
                await _dataClient.Events.GetDependencyEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        /// <summary>
        /// Gets the currently available requests tracked on Application Insights.
        /// </summary>
        /// <exception cref="NotEmptyException">Thrown when the retrieved telemetry doesn't contain any items.</exception>
        public async Task<EventsRequestResult[]> GetRequestsAsync()
        {
            EventsResults<EventsRequestResult> result = 
                await _dataClient.Events.GetRequestEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        /// <summary>
        /// Gets the currently available metrics tracked on Application Insights.
        /// </summary>
        /// <param name="body">The schema body content of the request to filter out metrics.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> is <c>null</c>.</exception>
        /// <exception cref="NotEmptyException">Thrown when the retrieved telemetry doesn't contain any items.</exception>
        public async Task<MetricsResultsItem[]> GetMetricsAsync(MetricsPostBodySchema body)
        {
            Guard.NotNull(body, nameof(body), "Requires a metrics body schema to request specific metrics from Application Insights");

            IList<MetricsResultsItem> items = 
                await _dataClient.Metrics.GetMultipleAsync(_applicationId, new List<MetricsPostBodySchema> { body });

            return items.ToArray();
        }

        /// <summary>
        /// Gets the currently available exceptions tracked on Application Insights.
        /// </summary>
        /// <exception cref="NotEmptyException">Thrown when the retrieved telemetry doesn't contain any items.</exception>
        public async Task<EventsExceptionResult[]> GetExceptionsAsync()
        {
            EventsResults<EventsExceptionResult> result = 
                await _dataClient.Events.GetExceptionEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        /// <summary>
        /// Gets the currently available traces tracked on Application Insights.
        /// </summary>
        /// <exception cref="NotEmptyException">Thrown when the retrieved telemetry doesn't contain any items.</exception>
        public async Task<EventsTraceResult[]> GetTracesAsync()
        {
            EventsResults<EventsTraceResult> result = 
                await _dataClient.Events.GetTraceEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        /// <summary>
        /// Gets the currently available events tracked on Application Insights.
        /// </summary>
        /// <exception cref="NotEmptyException">Thrown when the retrieved telemetry doesn't contain any items.</exception>
        public async Task<EventsCustomEventResult[]> GetEventsAsync()
        {
            EventsResults<EventsCustomEventResult> result = 
                await _dataClient.Events.GetCustomEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }
    }
}