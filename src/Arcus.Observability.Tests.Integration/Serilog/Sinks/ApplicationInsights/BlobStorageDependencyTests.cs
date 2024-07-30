using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class BlobStorageDependencyTests : ApplicationInsightsSinkTests
    {
        public BlobStorageDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogBlobStorageDependency_SinksToApplicationInsights_ResultsInBlobStorageDependencyTelemetry()
        {
            // Arrange
            string componentName = BogusGenerator.Commerce.ProductName();
            LoggerConfiguration.Enrich.WithComponentName(componentName);

            string dependencyType = "Azure blob";
            string containerName = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            string dependencyName = $"{accountName}/{containerName}";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                DependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Type);
                    Assert.Equal(accountName, result.Target);
                    Assert.Equal(containerName, result.Data);
                    Assert.Equal(componentName, result.RoleName);
                    Assert.Equal(dependencyName, result.Name);
                    Assert.Equal(dependencyId, result.Id);
                });
            });
        }
    }
}
