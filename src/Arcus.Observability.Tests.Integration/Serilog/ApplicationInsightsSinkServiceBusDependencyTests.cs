﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Azure.ApplicationInsights.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog 
{
    public class ApplicationInsightsSinkServiceBusDependencyTests : ApplicationInsightsSinkTests
    {
        public ApplicationInsightsSinkServiceBusDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogServiceBusDependency_SinksToApplicationInsights_ResultsInServiceBusDependencyTelemetry()
        {
            // Arrange
            string entityName = BogusGenerator.Commerce.Product();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset(days: 0);
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
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result => result.Dependency.Type == "Azure Service Bus" && result.Dependency.Target == entityName);
                });
            }
        }
    }
}