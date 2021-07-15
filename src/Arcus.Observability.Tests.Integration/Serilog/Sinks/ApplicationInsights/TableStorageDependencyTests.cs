using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog.Events;
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
            string dependencyType = "Azure table";
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
                        await client.Events.GetDependencyEventsAsync(ApplicationId, "PT30M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == dependencyType
                               && result.Dependency.Target == accountName
                               && result.Dependency.Data == tableName
                               && result.Cloud.RoleName == componentName;
                    });
                });
            }

            AssertX.Any(GetLogEventsFromMemory(), logEvent => {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);

                var actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(dependencyType, actualDependencyType.Value.ToDecentString());

                var actualDependencyData = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyData));
                Assert.Equal(tableName, actualDependencyData.Value.ToDecentString());

                var actualTargetName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(accountName, actualTargetName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}