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
    public class CustomDependencyTests : ApplicationInsightsSinkTests
    {
        public CustomDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Arcus";
            string dependencyData = BogusGenerator.Finance.Account();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();
                
                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, timespan: "PT30M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Dependency.Type == dependencyType && result.Dependency.Data == dependencyData);
                });
            }
            
            Assert.Contains(GetLogEventsFromMemory(), logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                return logEntry != null &&
                       logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.DependencyType))?.Value.ToDecentString() == dependencyType &&
                       logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.DependencyData))?.Value.ToDecentString() == dependencyData &&
                       logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.Context)) != null;
            });
        }
    }
}