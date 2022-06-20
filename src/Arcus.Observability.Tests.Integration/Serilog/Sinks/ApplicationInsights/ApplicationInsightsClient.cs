using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class ApplicationInsightsClient
    {
        private const string PastHalfHourTimeSpan = "PT30M";

        private readonly IApplicationInsightsDataClient _dataClient;
        private readonly string _applicationId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsClient" /> class.
        /// </summary>
        public ApplicationInsightsClient(IApplicationInsightsDataClient dataClient, string applicationId)
        {
            _dataClient = dataClient;
            _applicationId = applicationId;
        }

        public async Task<EventsDependencyResult[]> GetDependenciesAsync()
        {
            EventsResults<EventsDependencyResult> result = 
                await _dataClient.Events.GetDependencyEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        public async Task<EventsRequestResult[]> GetRequestsAsync()
        {
            EventsResults<EventsRequestResult> result = 
                await _dataClient.Events.GetRequestEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        public async Task<EventsCustomMetricResult[]> GetMetricsAsync()
        {
            EventsResults<EventsCustomMetricResult> result = 
                await _dataClient.Events.GetCustomMetricEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        public async Task<EventsExceptionResult[]> GetExceptionsAsync()
        {
            EventsResults<EventsExceptionResult> result = 
                await _dataClient.Events.GetExceptionEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        public async Task<EventsTraceResult[]> GetTracesAsync()
        {
            EventsResults<EventsTraceResult> result = 
                await _dataClient.Events.GetTraceEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }

        public async Task<EventsCustomEventResult[]> GetEventsAsync()
        {
            EventsResults<EventsCustomEventResult> result = 
                await _dataClient.Events.GetCustomEventsAsync(_applicationId, timespan: PastHalfHourTimeSpan);

            Assert.NotEmpty(result.Value);
            return result.Value.ToArray();
        }
    }
}