using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.Extensions.Logging;
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
            string column = BogusGenerator.Database.Column();
            string sqlCommand = $"SELECT {column} FROM Some_Table";
            string operationName = $"Get all {column}";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogSqlDependency(serverName, databaseName, sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                DependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Type, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(serverName, result.Target);
                    Assert.Contains(databaseName, result.Name);
                    Assert.Contains(operationName, result.Name);
                    Assert.Equal(dependencyId, result.Id);
                });
            });
        }

        [Fact]
        public async Task LogSqlDependencyWithConnectionString_SinksToApplicationInsights_ResultsInSqlDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "SQL";
            string serverName = BogusGenerator.Database.Engine();
            string databaseName = BogusGenerator.Database.Collation();
            var connectionString = $"Server={serverName};Database={databaseName};User=admin;Password=123";
            string column = BogusGenerator.Database.Column();
            string sqlCommand = $"SELECT {column} FROM Some_Table";
            string operationName = $"Get all {column}";
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogSqlDependencyWithConnectionString(connectionString, sqlCommand: sqlCommand, operationName, isSuccessful, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                DependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Type, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(serverName, result.Target);
                    Assert.Contains(databaseName, result.Name);
                    Assert.Contains(operationName, result.Name);
                    Assert.Equal(dependencyId, result.Id);
                });
            });
        }
    }
}