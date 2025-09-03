using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class CustomRequestTests : ApplicationInsightsSinkTests
    {
        public CustomRequestTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogCustomRequest_SinksToApplicationInsights_ResultsInCustomRequestTelemetry()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            LoggerConfiguration.Enrich.WithComponentName(componentName);

            string customRequestSource = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();

            bool isSuccessful = BogusGenerator.Random.Bool();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.Now;
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, startTime, duration, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] requests = await client.GetRequestsAsync();
                AssertX.Any(requests, result =>
                {
                    Assert.Equal(operationName, result.Request.Name);
                    Assert.Contains(customRequestSource, result.Request.Source);
                    Assert.True(string.IsNullOrWhiteSpace(result.Request.Url), "request URL should be blank");
                    Assert.Equal(operationName, result.Operation.Name);
                    Assert.Equal(isSuccessful, result.Success);
                    Assert.Equal(componentName, result.Cloud.RoleName);
                });
            });
        }
    }
}
