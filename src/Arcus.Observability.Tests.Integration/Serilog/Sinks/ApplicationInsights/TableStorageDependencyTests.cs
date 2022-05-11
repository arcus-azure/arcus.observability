using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            string dependencyName = $"{accountName}/{tableName}";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, timespan: "PT30M");
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(dependencyType, result.Dependency.Type);
                        Assert.Equal(accountName, result.Dependency.Target);
                        Assert.Equal(tableName, result.Dependency.Data);
                        Assert.Equal(componentName, result.Cloud.RoleName);
                        Assert.Equal(dependencyName, result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                    });
                });
            }

            AssertSerilogLogProperties(dependencyType, tableName, accountName, dependencyName);
        }

        private void AssertSerilogLogProperties(
            string dependencyType,
            string tableName,
            string accountName,
            string dependencyName)
        {
            IEnumerable<LogEvent> logEvents = GetLogEventsFromMemory();
            AssertX.Any(logEvents, logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);
            
                LogEventProperty actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(dependencyType, actualDependencyType.Value.ToDecentString());
            
                LogEventProperty actualDependencyData = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyData));
                Assert.Equal(tableName, actualDependencyData.Value.ToDecentString());
            
                LogEventProperty actualTargetName =
                    Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(accountName, actualTargetName.Value.ToDecentString());
            
                LogEventProperty actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal(dependencyName, actualDependencyName.Value.ToDecentString());
            
                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}