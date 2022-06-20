﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class AzureKeyVaultDependencyTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkTests"/> class.
        /// </summary>
        public AzureKeyVaultDependencyTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogAzureKeyVaultDependency_SinksToApplicationInsights_ResultsInAzureKeyVaultDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure key vault";
            string vaultUri = "https://myvault.vault.azure.net";
            string secretName = "MySecret";
            string dependencyName = vaultUri;
            string dependencyId = Guid.NewGuid().ToString();

            bool isSuccessful = BogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan duration = BogusGenerator.Date.Timespan();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

            Logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration, dependencyId, telemetryContext);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                Assert.Contains(results, result =>
                {
                    return result.Dependency.Type == dependencyType
                           && result.Dependency.Id == dependencyId
                           && result.Dependency.Target == vaultUri
                           && result.Dependency.Data == secretName
                           && result.Dependency.Name == dependencyName;
                });
            });
        }
    }
}
