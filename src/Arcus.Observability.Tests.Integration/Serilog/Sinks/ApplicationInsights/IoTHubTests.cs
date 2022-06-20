using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class IoTHubTests : ApplicationInsightsSinkTests
    {
        public IoTHubTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogIoTHubDependency_SinksToApplicationInsights_ResultsIEventHubsDependencyTelemetry()
        {
            // Arrange
            string dependencyType = "Azure IoT Hub";
            string componentName = BogusGenerator.Commerce.ProductName();
            string iotHubName = BogusGenerator.Commerce.ProductName();
            string dependencyName = iotHubName;
            string dependencyId = BogusGenerator.Random.Guid().ToString();

            using (ILoggerFactory loggerFactory = CreateLoggerFactory(config => config.Enrich.WithComponentName(componentName)))
            {
                ILogger logger = loggerFactory.CreateLogger<ApplicationInsightsSinkTests>();

                bool isSuccessful = BogusGenerator.PickRandom(true, false);
                DateTimeOffset startTime = DateTimeOffset.Now;
                TimeSpan duration = BogusGenerator.Date.Timespan();
                Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();

                // Act
                logger.LogIotHubDependency(iotHubName, isSuccessful, startTime, duration, dependencyId, telemetryContext);
            }

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsDependencyResult[] results = await client.GetDependenciesAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(dependencyType, result.Dependency.Type);
                    Assert.Equal(iotHubName, result.Dependency.Target);
                    Assert.Equal(componentName, result.Cloud.RoleName);
                    Assert.Equal(dependencyName, result.Dependency.Name);
                    Assert.Equal(dependencyId, result.Dependency.Id);
                });
            });
        }
    }
}
