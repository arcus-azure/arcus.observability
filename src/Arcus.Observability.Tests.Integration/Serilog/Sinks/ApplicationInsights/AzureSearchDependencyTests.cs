﻿using System;
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

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class AzureSearchDependencyTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public AzureSearchDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogAzureSearchDependency_SinksToApplicationInsights_ResultsInAzureSearchDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure Search";
            string searchServiceName = BogusGenerator.Commerce.Product();
            string operationName = BogusGenerator.Commerce.ProductName();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (IApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = 
                        await client.Events.GetDependencyEventsAsync(ApplicationId, timespan: "PT30M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == dependencyType
                               && result.Dependency.Target == searchServiceName
                               && result.Dependency.Data == operationName;
                    });
                });
            }

            Assert.Contains(GetLogEventsFromMemory(), logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                return logEntry != null
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.DependencyType))?.Value.ToDecentString() == dependencyType
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.TargetName))?.Value.ToDecentString() == searchServiceName
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.DependencyData))?.Value.ToDecentString() == operationName
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.Context)) != null;
            });
        }
    }
}
