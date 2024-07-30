using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.Extensions.Logging;
using Serilog;
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
        public async Task LogServiceBusDependencyWithCorrelation_SinksToApplicationInsights_ResultsInServiceBusDependencyTelemetry()
        {
            // Arrange
            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor);

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
                DependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Type);
                    Assert.Contains(entityName, result.Target);
                    Assert.Equal(dependencyName, result.Name);
                    Assert.Equal(dependencyId, result.Id);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.EntityType, entityType.ToString());
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.Endpoint, namespaceEndpoint);

                    Assert.Equal(correlation.OperationId, result.Operation.ParentId);
                    Assert.Equal(correlation.TransactionId, result.Operation.Id);
                });
            });
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
                DependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Type);
                    Assert.Contains(entityName, result.Target);
                    Assert.Equal(dependencyName, result.Name);
                    Assert.Equal(dependencyId, result.Id);

                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.EntityType, entityType.ToString());
                    AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.Endpoint, namespaceEndpoint);
                });
            });
        }

        private static void AssertContainsCustomDimension(IDictionary<string, string> customDimensions, string key, string expected)
        {
            Assert.True(customDimensions.TryGetValue(key, out string actual), $"Cannot find {key} in custom dimensions: {String.Join(", ", customDimensions.Keys)}");
            Assert.Equal(expected, actual);
        }
    }
}