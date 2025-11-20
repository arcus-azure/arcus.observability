using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Xunit;

namespace Arcus.Observability.Tests.Unit.OpenTelemetry
{
    public class OpenTelemetryObservabilityMetricsTests
    {
        private static readonly Faker Bogus = new();

        [Fact]
        public async Task RecordMetric_WithDefault_TracksMetric()
        {
            // Arrange
            var metricName = string.Join("_", Bogus.Lorem.Words().Select(w => w.ToLowerInvariant()));
            var metricValue = Bogus.Random.Int();

            await using var context = await MetricsTestContext.GivenAsync(otel =>
            {
                otel.UseObservability();
            });

            // Act
            context.WhenObservability(obs => obs.RecordMetric(metricName).WithValue(metricValue));

            // Assert
            context.ShouldFindSingleMetric(metricName, metricValue);
        }

        private static Dictionary<string, object> CreateTelemetryContext()
        {
            return Bogus.Make(Bogus.Random.Int(1, 5), () => new KeyValuePair<string, object>(Bogus.Random.Guid().ToString(), Bogus.Random.Int()))
                        .ToDictionary(item => item.Key, item => item.Value);
        }

        private sealed class MetricsTestContext : IAsyncDisposable
        {
            private readonly IHost _host;
            private readonly Collection<Metric> _actualMetrics;


            /// <summary>
            /// Initializes a new instance of the <see cref="MetricsTestContext"/> class.
            /// </summary>
            private MetricsTestContext(IHost host, Collection<Metric> actualMetrics)
            {
                _host = host;
                _actualMetrics = actualMetrics;
            }

            public static async Task<MetricsTestContext> GivenAsync(Action<OpenTelemetryBuilder> configureMetrics)
            {
                var builder = Host.CreateDefaultBuilder();

                var actualMetrics = new Collection<Metric>();
                builder.ConfigureServices(services =>
                {
                    configureMetrics(
                        services.AddOpenTelemetry()
                                .WithMetrics(metrics => metrics.AddInMemoryExporter(actualMetrics)));
                });

                var host = builder.Build();
                await host.StartAsync(TestContext.Current.CancellationToken);

                return new(host, actualMetrics);
            }

            public void WhenObservability(Action<IObservability> act)
            {
                act(_host.Services.GetRequiredService<IObservability>());
            }

            public void ShouldFindSingleMetric(string metricName, double metricValue)
            {
                _host.Services.GetRequiredService<MeterProvider>().ForceFlush();

                Metric actual = Assert.Single(_actualMetrics, m => m.Name == metricName);
                Assert.Equal(metricName, actual.Name);

                foreach (var m in actual.GetMetricPoints())
                {
                    double actualValue = m.GetSumLong();
                    Assert.Equal(metricValue, actualValue);
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or
            /// resetting unmanaged resources asynchronously.</summary>
            /// <returns>A task that represents the asynchronous dispose operation.</returns>
            public async ValueTask DisposeAsync()
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
    }
}
