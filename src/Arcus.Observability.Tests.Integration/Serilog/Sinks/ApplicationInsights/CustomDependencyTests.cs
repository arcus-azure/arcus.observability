using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
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
        public async Task LogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetryWithDependencyName()
        {
            // Arrange
            string dependencyType = "Arcus";
            string dependencyData = BogusGenerator.Finance.Account();
            string dependencyName = BogusGenerator.Random.Word();
            string targetName = BogusGenerator.Random.Word();
            string dependencyId = Guid.NewGuid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> userContext = CreateTestTelemetryContext();
            var telemetryContext = new SerilogDependencyTelemetryContext(userContext)
            {
                DependencyData = dependencyData,
                DependencyType = dependencyType,
                TargetName = targetName,
                DependencyId = dependencyId
            };

            // Act
            Logger.LogCustomDependency(dependencyName, isSuccessful, startTime, duration, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Equal(dependencyData, result.Dependency.Data);
                    Assert.Equal(dependencyName, result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                    Assert.Equal(isSuccessful, result.Success);
                    Assert.Equal(targetName, result.Dependency.Target);
                    Assert.All(telemetryContext, item => Assert.Equal(item.Value.ToString(), Assert.Contains(item.Key, result.CustomDimensions)));
                });
            });
        }

        [Fact]
        public async Task LogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Arcus";
            string dependencyData = BogusGenerator.Finance.Account();
            string dependencyId = Guid.NewGuid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogDependency(dependencyType, dependencyData, isSuccessful, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Equal(dependencyData, result.Dependency.Data);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                });
            });
        }

        [Fact]
        public async Task DeprecatedLogDependency_SinksToApplicationInsights_ResultsInDependencyTelemetryWithDependencyName()
        {
            // Arrange
            string dependencyType = "Arcus";
            string dependencyData = BogusGenerator.Finance.Account();
            string dependencyName = BogusGenerator.Random.Word();
            string dependencyId = Guid.NewGuid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogDependency(dependencyType, dependencyData, isSuccessful, dependencyName, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Equal(dependencyData, result.Dependency.Data);
                    Assert.Equal(dependencyName, result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                    Assert.Equal(isSuccessful, result.Success);
                    Assert.All(telemetryContext, item => Assert.Equal(item.Value.ToString(), Assert.Contains(item.Key, result.CustomDimensions)));
                });
            });
        }
    }
}