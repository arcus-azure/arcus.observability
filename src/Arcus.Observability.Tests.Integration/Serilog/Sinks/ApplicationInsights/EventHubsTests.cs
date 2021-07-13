﻿using System;
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
            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, timespan: "PT30M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == dependencyType
                               && result.Dependency.Target == eventHubName
                               && result.Dependency.Data == namespaceName
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
                Assert.Equal(namespaceName, actualDependencyData.Value.ToDecentString());

                var actualTargetName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(eventHubName, actualTargetName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}
