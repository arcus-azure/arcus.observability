using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EventHubsTests : ApplicationInsightsSinkTests
    {
        public EventHubsTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogEventHubsDependency_SinksToApplicationInsights_ResultsIEventHubsDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure Event Hubs";
            string componentName = BogusGenerator.Commerce.ProductName();
            string eventHubName = BogusGenerator.Commerce.ProductName();
            string namespaceName = BogusGenerator.Finance.AccountName();
            string dependencyName = eventHubName;
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(dependencyType, result.Dependency.Type);
                        Assert.Equal(eventHubName, result.Dependency.Target);
                        Assert.Equal(namespaceName, result.Dependency.Data);
                        Assert.Equal(componentName, result.Cloud.RoleName);
                        Assert.Equal(dependencyName, result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                    });
                });
            }

            AssertSerilogLogProperties(dependencyType, namespaceName, eventHubName, dependencyName);
        }

        private void AssertSerilogLogProperties(
            string dependencyType,
            string namespaceName,
            string eventHubName,
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
                Assert.Equal(namespaceName, actualDependencyData.Value.ToDecentString());

                LogEventProperty actualTargetName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(eventHubName, actualTargetName.Value.ToDecentString());

                LogEventProperty actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal(dependencyName, actualDependencyName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}
