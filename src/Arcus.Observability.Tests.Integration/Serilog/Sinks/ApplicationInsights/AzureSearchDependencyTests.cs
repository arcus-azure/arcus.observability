﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class AzureSearchDependencyTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public AzureSearchDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogAzureSearchDependency_SinksToApplicationInsights_ResultsInAzureSearchDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure Search";
            string searchServiceName = BogusGenerator.Commerce.Product();
            string operationName = BogusGenerator.Commerce.ProductName();
            string dependencyName = searchServiceName;
            string dependencyId = BogusGenerator.Lorem.Word();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            // Act
            Logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                    Assert.Equal(searchServiceName, result.Dependency.Target);
                    Assert.Equal(operationName, result.Dependency.Data);
                    Assert.Equal(dependencyName, result.Dependency.Name);
                });
            });
        }
    }
}
