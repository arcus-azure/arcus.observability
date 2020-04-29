using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Azure.ApplicationInsights.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog 
{
    public class ApplicationInsightsSinkSqlDependencyTests : ApplicationInsightsSinkTests
    {
        public ApplicationInsightsSinkSqlDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogSqlDependency_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            string serverName = BogusGenerator.Database.Engine();
            string databaseName = BogusGenerator.Database.Collation();
            string tableName = BogusGenerator.Database.Column();
            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                string operation = BogusGenerator.PickRandom("GET", "UPDATE", "DELETE", "CREATE");
                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = BogusGenerator.Date.RecentOffset(days: 0);
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogSqlDependency(serverName, databaseName, tableName, operation, isSuccessful, startTime, duration, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = await client.GetDependencyEventsAsync();
                    Assert.NotEmpty(results.Value);
                    Assert.Contains(results.Value, result =>
                    {
                        return result.Dependency.Type == "SQL"
                               && result.Dependency.Target == serverName
                               && result.Dependency.Name == $"SQL: {databaseName}/{tableName}";
                    });
                });
            }
        }
    }
}