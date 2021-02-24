using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class AzureKeyVaultDependencyTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public AzureKeyVaultDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogAzureKeyVaultDependency_SinksToApplicationInsights_ResultsInAzureKeyVaultDependencyTelemetry()
        {
            // Arrange
            string vaultUri = "https://myvault.vault.azure.net";
            string secretName = "MySecret";

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<AzureKeyVaultDependencyTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (IApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = 
                        await client.Events.GetDependencyEventsAsync(ApplicationId, timespan: "PT10M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "Azure key vault"
                               && result.Dependency.Target == vaultUri
                               && result.Dependency.Data == secretName;
                    });
                });
            }
        }
    }
}
