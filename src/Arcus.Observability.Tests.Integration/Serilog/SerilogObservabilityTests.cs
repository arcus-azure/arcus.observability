using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Bogus;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    public class SerilogObservabilityTests
    {
        private static readonly Faker Bogus = new();

        [Fact]
        public async Task RecordMetric_WithDefault_TracksMetric()
        {
            // Arrange
            using var context = SerilogObservabilityTestContext.Given(services =>
            {
                services.UseArcusObservability();
            });

            var metricName = string.Join("_", Bogus.Lorem.Words().Select(w => w.ToLowerInvariant()));
            var metricValue = Bogus.Random.Int();
            var telemetryContext = CreateTelemetryContext();

            // Act
            context.WhenObservability(obs => obs.RecordMetric(metricName).WithValue(metricValue, telemetryContext));

            // Assert
            await context.ShouldFindSingleMetricAsync(metricName, metricValue, telemetryContext);
        }

        [Fact]
        public async Task StartRequest_WithStatus_TracksRequest()
        {
            // Arrange
            using var context = SerilogObservabilityTestContext.Given(services =>
            {
                services.UseArcusObservability();
            });

            var operationName = Bogus.Random.Guid().ToString();
            var isSuccessful = Bogus.Random.Bool();
            var telemetryContext = CreateTelemetryContext();

            // Act
            context.WhenObservability(obs =>
            {
                using var req = obs.StartCustomRequest(operationName, telemetryContext);
                req.IsSuccessful = isSuccessful;
            });

            // Assert
            await context.ShouldFindSingleRequestAsync(operationName, isSuccessful, telemetryContext);
        }

        [Fact]
        public async Task StartDependency_WithStatus_TracksDependency()
        {
            // Arrange
            using var context = SerilogObservabilityTestContext.Given(services =>
            {
                services.UseArcusObservability();
            });

            var dependencyName = Bogus.Random.Guid().ToString();
            var isSuccessful = Bogus.Random.Bool();
            var telemetryContext = CreateTelemetryContext();

            // Act
            context.WhenObservability(obs =>
            {
                using var req = obs.StartCustomDependency(dependencyName, telemetryContext);
                req.IsSuccessful = isSuccessful;
            });

            // Assert
            await context.ShouldFindSingleDependencyAsync(dependencyName, isSuccessful, telemetryContext);
        }

        private static Dictionary<string, object> CreateTelemetryContext()
        {
            return Bogus.Make(Bogus.Random.Int(1, 5), () => new KeyValuePair<string, object>(Bogus.Random.Guid().ToString(), Bogus.Random.Int()))
                        .ToDictionary(item => item.Key, item => item.Value);
        }

        private sealed class SerilogObservabilityTestContext : IDisposable
        {
            private readonly InMemoryApplicationInsightsTelemetryConverter _spySink;
            private readonly IServiceProvider _provider;
            private readonly Logger _logger;

            private SerilogObservabilityTestContext(InMemoryApplicationInsightsTelemetryConverter spySink, IServiceProvider provider, Logger logger)
            {
                _spySink = spySink;
                _provider = provider;
                _logger = logger;
            }

            public static SerilogObservabilityTestContext Given(Action<IServiceCollection> configureObservability)
            {
                var spySink = new InMemoryApplicationInsightsTelemetryConverter();
                var config = new LoggerConfiguration().WriteTo.ApplicationInsights(spySink);
                var logger = config.CreateLogger();

                var services = new ServiceCollection();
                services.AddLogging(logging => logging.AddSerilog(logger));
                configureObservability(services);
                var provider = services.BuildServiceProvider();

                return new SerilogObservabilityTestContext(spySink, provider, logger);
            }

            public void WhenObservability(Action<IObservability> act)
            {
                act(_provider.GetRequiredService<IObservability>());
            }

            public async Task ShouldFindSingleMetricAsync(string metricName, double metricValue, IDictionary<string, object> telemetryContext)
            {
                var client = new InMemoryTelemetryQueryClient(_spySink);
                EventsMetricsResult[] results = await client.GetMetricsAsync(metricName);
                Assert.NotEmpty(results);
                AssertX.Any(results, metric =>
                {
                    Assert.Equal(metricName, metric.Name);
                    Assert.Equal(metricValue, metric.Value);
                    Assert.All(telemetryContext, item =>
                    {
                        string actual = Assert.Contains(item.Key, metric.CustomDimensions);
                        Assert.Equal(item.Value.ToString(), actual);
                    });
                });
            }

            public async Task ShouldFindSingleRequestAsync(string operationName, bool isSuccessful, IDictionary<string, object> telemetryContext)
            {
                var client = new InMemoryTelemetryQueryClient(_spySink);
                var results = await client.GetRequestsAsync();
                Assert.NotEmpty(results);
                AssertX.Any(results, request =>
                {
                    Assert.Equal(operationName, request.Operation.Name);
                    Assert.Equal(isSuccessful, request.Success);
                    Assert.All(telemetryContext, item =>
                    {
                        string actual = Assert.Contains(item.Key, request.CustomDimensions);
                        Assert.Equal(item.Value.ToString(), actual);
                    });
                });
            }

            public async Task ShouldFindSingleDependencyAsync(string dependencyName, bool isSuccessful, IDictionary<string, object> telemetryContext)
            {
                var client = new InMemoryTelemetryQueryClient(_spySink);
                var results = await client.GetDependenciesAsync();
                Assert.NotEmpty(results);
                AssertX.Any(results, dependency =>
                {
                    Assert.Equal(dependencyName, dependency.Dependency.Name);
                    Assert.Equal(isSuccessful, dependency.Success);
                    Assert.All(telemetryContext, item =>
                    {
                        string actual = Assert.Contains(item.Key, dependency.CustomDimensions);
                        Assert.Equal(item.Value.ToString(), actual);
                    });
                });
            }

            public void Dispose()
            {
                _logger.Dispose();
            }
        }
    }
}
