using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using LoggerEnrichmentConfigurationExtensions = Serilog.LoggerEnrichmentConfigurationExtensions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights 
{
    public class TableStorageDependencyTests : ApplicationInsightsSinkTests
    {
        public TableStorageDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogTableStorageDependency_SinksToApplicationInsights_ResultsInTableStorageDependencyTelemetry()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            string tableName = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => LoggerEnrichmentConfigurationExtensions.WithComponentName(config.Enrich, componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = 
                        await client.Events.GetDependencyEventsAsync(ApplicationId, "PT10M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "Azure table"
                               && result.Dependency.Target == accountName
                               && result.Dependency.Data == tableName
                               && result.Cloud.RoleName == componentName;
                    });
                });
            }
        }
    }
}