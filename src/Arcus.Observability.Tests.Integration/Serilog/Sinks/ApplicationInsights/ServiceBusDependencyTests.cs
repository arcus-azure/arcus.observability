using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
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

            string dependencyType = "Azure Service Bus";
            string namespaceEndpoint = BogusGenerator.Commerce.Product();
            string entityName = BogusGenerator.Commerce.Product();
            string dependencyName = entityName;
            string dependencyId = BogusGenerator.Lorem.Word();
            var entityType = ServiceBusEntityType.Queue;

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(configureLogging: config => config.Enrich.WithCorrelationInfo(accessor)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogServiceBusDependency(namespaceEndpoint, entityName, isSuccessful, startTime, duration, dependencyId, entityType, telemetryContext);
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
                        Assert.Contains(namespaceEndpoint, result.Dependency.Target);
                        Assert.Contains(entityName, result.Dependency.Target);
                        Assert.Equal(dependencyName, result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);

                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.EntityType, entityType.ToString());

                        Assert.Equal(correlation.OperationId, result.Operation.ParentId);
                        Assert.Equal(correlation.TransactionId, result.Operation.Id);
                    });
                });
            }

            AssertSerilogLogProperties(dependencyType, namespaceEndpoint, entityName, dependencyName);
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

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogServiceBusDependency(namespaceEndpoint, entityName, isSuccessful, startTime, duration, dependencyId, entityType, telemetryContext);
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
                        Assert.Contains(namespaceEndpoint, result.Dependency.Target);
                        Assert.Contains(entityName, result.Dependency.Target);
                        Assert.Equal(dependencyName, result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);

                        AssertContainsCustomDimension(result.CustomDimensions, ContextProperties.DependencyTracking.ServiceBus.EntityType, entityType.ToString());
                    });
                });
            }

            AssertSerilogLogProperties(dependencyType, namespaceEndpoint, entityName, dependencyName);
        }

        private static void AssertContainsCustomDimension(EventsResultDataCustomDimensions customDimensions, string key, string expected)
        {
            Assert.True(customDimensions.TryGetValue(key, out string actual), $"Cannot find {key} in custom dimensions: {String.Join(", ", customDimensions.Keys)}");
            Assert.Equal(expected, actual);
        }

        private void AssertSerilogLogProperties(string dependencyType, string namespaceEndpoint, string entityName, string dependencyName)
        {
            IEnumerable<LogEvent> logEvents = GetLogEventsFromMemory();
            AssertX.Any(logEvents, logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);

                LogEventProperty actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(dependencyType, actualDependencyType.Value.ToDecentString());

                LogEventProperty actualTargetNameProperty = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                string actualTargetName = actualTargetNameProperty.Value.ToDecentString();
                Assert.Contains(namespaceEndpoint, actualTargetName);
                Assert.Contains(entityName, actualTargetName);

                LogEventProperty actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal(dependencyName, actualDependencyName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}