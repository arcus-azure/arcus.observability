using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
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
            string namespaceEndpoint = BogusGenerator.Commerce.Product();
            string entityName = BogusGenerator.Commerce.Product();
            string dependencyName = entityName;
            string dependencyId = BogusGenerator.Lorem.Word();
            var entityType = ServiceBusEntityType.Queue;

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogServiceBusDependency(namespaceEndpoint, entityName, isSuccessful, startTime, duration, dependencyId, entityType, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Contains(namespaceEndpoint, result.Dependency.Target);
                    Assert.Contains(entityName, result.Dependency.Target);
                    Assert.Equal(dependencyName, result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.EntityType, entityType.ToString());
                });
            });
        }

        private static void AssertContainsCustomDimension(EventsResultDataCustomDimensions customDimensions, string key, string expected)
        {
            Assert.True(customDimensions.TryGetValue(key, out string actual), $"Cannot find {key} in custom dimensions: {String.Join(", ", customDimensions.Keys)}");
            Assert.Equal(expected, actual);
        }
    }
}