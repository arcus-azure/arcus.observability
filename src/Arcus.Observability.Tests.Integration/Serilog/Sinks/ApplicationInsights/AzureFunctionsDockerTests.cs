using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    [Collection("Docker")]
    [Trait("Category", "Docker")]
    public class AzureFunctionsDockerTests : ApplicationInsightsSinkTests
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsDockerTests" /> class.
        /// </summary>
        public AzureFunctionsDockerTests(ITestOutputHelper outputWriter) 
            : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogRequest_WithRequestsOperationName_SinksToApplicationInsights()
        {
            // Arrange
            int httpPort = Configuration.GetValue<int>("AzureFunctions:HttpPort");
            string? requestUri = $"http://localhost:{httpPort}/api/order";
            Logger.LogInformation("GET -> {URI}", requestUri);

            using (HttpResponseMessage response = await HttpClient.GetAsync(requestUri))
            {
                Logger.LogInformation("{StatusCode} <- {URI}", response.StatusCode, requestUri);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
                {
                    await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                    {
                        EventsResults<EventsRequestResult> results = await client.Events.GetRequestEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                        Assert.NotEmpty(results.Value);
                        AssertX.Any(results.Value, result =>
                        {
                            Assert.Contains("order", result.Request.Url);
                            Assert.Equal(200.ToString(), result.Request.ResultCode);
                            Assert.True(Guid.TryParse(result.Request.Id, out Guid _));
                            Assert.Equal(HttpMethod.Get.Method + " /api/order", result.Operation.Name);
                        });
                    });
                }
            }
        }
    }
}
