using System;
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
    public class SqlDependencyTests : ApplicationInsightsSinkTests
    {
        public SqlDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogSqlDependency_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "SQL";
            string serverName = BogusGenerator.Database.Engine();
            string databaseName = BogusGenerator.Database.Collation();
            string sqlCommand = $"SELECT {BogusGenerator.Database.Column()} FROM Some_Table";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogSqlDependency(serverName, databaseName, sqlCommand, isSuccessful, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results = 
                        await client.Events.GetDependencyEventsAsync(ApplicationId, PastHalfHourTimeSpan);
                   
                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(dependencyType, result.Dependency.Type);
                        Assert.Equal(serverName, result.Dependency.Target);
                        Assert.Equal($"{dependencyType}: {databaseName}", result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                    });
                });
            }

            AssertSerilogLogEvents(dependencyType, serverName, databaseName);
        }

        [Fact]
        public async Task LogSqlDependencyWithConnectionString_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "SQL";
            string serverName = BogusGenerator.Database.Engine();
            string databaseName = BogusGenerator.Database.Collation();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string sqlCommand = $"SELECT {BogusGenerator.Database.Column()} FROM Some_Table";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogSqlDependency(connectionString, sqlCommand, isSuccessful, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            using (ApplicationInsightsDataClient client = CreateApplicationInsightsClient())
            {
                await RetryAssertUntilTelemetryShouldBeAvailableAsync(async () =>
                {
                    EventsResults<EventsDependencyResult> results =
                        await client.Events.GetDependencyEventsAsync(ApplicationId, PastHalfHourTimeSpan);

                    Assert.NotEmpty(results.Value);
                    AssertX.Any(results.Value, result =>
                    {
                        Assert.Equal(dependencyType, result.Dependency.Type);
                        Assert.Equal(serverName, result.Dependency.Target);
                        Assert.Equal($"{dependencyType}: {databaseName}", result.Dependency.Name);
                        Assert.Equal(dependencyId, result.Dependency.Id);
                    });
                });
            }

            AssertSerilogLogEvents(dependencyType, serverName, databaseName);
        }

        private void AssertSerilogLogEvents(string dependencyType, string serverName, string databaseName)
        {
            IEnumerable<LogEvent> logEvents = GetLogEventsFromMemory();
            AssertX.Any(logEvents, logEvent =>
            {
                StructureValue logEntry = logEvent.Properties.GetAsStructureValue(ContextProperties.DependencyTracking.DependencyLogEntry);
                Assert.NotNull(logEntry);

                LogEventProperty actualDependencyType = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyType));
                Assert.Equal(dependencyType, actualDependencyType.Value.ToDecentString(), true);

                LogEventProperty actualTargetName =
                    Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.TargetName));
                Assert.Equal(serverName, actualTargetName.Value.ToDecentString());

                LogEventProperty actualDependencyName = Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.DependencyName));
                Assert.Equal(databaseName, actualDependencyName.Value.ToDecentString());

                Assert.Single(logEntry.Properties, prop => prop.Name == nameof(DependencyLogEntry.Context));
            });
        }
    }
}