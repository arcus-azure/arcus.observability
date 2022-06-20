using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class TableStorageDependencyTests : ApplicationInsightsSinkTests
    {
        public TableStorageDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogTableStorageDependency_SinksToApplicationInsights_ResultsInTableStorageDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure table";
            string componentName = BogusGenerator.Commerce.ProductName();
            string tableName = BogusGenerator.Commerce.ProductName();
            string accountName = BogusGenerator.Finance.AccountName();
            string dependencyName = $"{accountName}/{tableName}";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Equal(accountName, result.Dependency.Target);
                    Assert.Equal(tableName, result.Dependency.Data);
                    Assert.Equal(componentName, result.Cloud.RoleName);
                    Assert.Equal(dependencyName, result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                });
            });
        }
    }
}