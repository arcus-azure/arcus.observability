using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    [Collection("Docker")]
    [Trait("Category", "Docker")]
    public class AzureFunctionsDockerTests : ApplicationInsightsSinkTests
    {
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
            TestLocation = TestLocation.Remote;

            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] results = await client.GetRequestsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal("200", result.Request.ResultCode);
                    Assert.Equal("Timer", result.Request.Source);
                    Assert.Equal("Triggered", result.Operation.Name);
                });
            });
        }
    }
}
