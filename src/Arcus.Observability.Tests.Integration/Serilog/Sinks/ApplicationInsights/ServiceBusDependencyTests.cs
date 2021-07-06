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
    public class ServiceBusDependencyTests : ApplicationInsightsSinkTests
    {
        public ServiceBusDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogServiceBusDependency_SinksToApplicationInsights_ResultsInServiceBusDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure Service Bus";
            string entityName = BogusGenerator.Commerce.Product();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogServiceBusDependency(entityName, isSuccessful, startTime, duration, ServiceBusEntityType.Queue, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.Events.GetDependencyEventsAsync(ApplicationId, timespan: "PT30M");
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Dependency.Type == dependencyType && result.Dependency.Target == entityName);
                });
            }

            Assert.Contains(GetLogEventsFromMemory(), logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                return logEntry != null
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.DependencyType))?.Value.ToDecentString() == dependencyType
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.TargetName))?.Value.ToDecentString() == entityName
                       && logEntry.Properties.FirstOrDefault(prop => prop.Name == nameof(DependencyLogEntry.Context)) != null;
            });
        }
    }
}