using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace Arcus.Observability.Tests.Unit.OpenTelemetry
{
    public class OpenTelemetryObservabilityOperationTests
    {
        private static readonly Faker Bogus = new();

        [Fact]
        public async Task StartRequest_ViaObservability_TracksRequestInOpenTelemetry()
        {
            // Arrange
            var operationName = Bogus.Random.Guid().ToString();
            var isSuccessful = Bogus.Random.Bool();
            var componentName = Bogus.Random.Guid().ToString().OrNull(Bogus);

            await using var context = await OpenTelemetryRequestTestContext.GivenAsync(otel =>
            {
                otel.UseArcusObservability(options =>
                {
                    if (componentName != null)
                    {
                        options.UseAppName(componentName);
                    }

                    options.UseAssemblyAppVersion<OpenTelemetryObservabilityOperationTests>();
                });
            });

            // Act
            context.WhenObservability(obs =>
            {
                using var req = obs.StartCustomRequest(operationName);
                req.IsSuccessful = isSuccessful;
            });

            // Assert
            context.ShouldFindSingleOperation(operationName, isSuccessful, componentName);
        }

        [Fact]
        public async Task StartDependency_ViaObservability_TracksDependencyInOpenTelemetry()
        {
            // Arrange
            var dependencyName = Bogus.Random.Guid().ToString();
            var isSuccessful = Bogus.Random.Bool();
            var componentName = Bogus.Random.Guid().ToString().OrNull(Bogus);

            await using var context = await OpenTelemetryRequestTestContext.GivenAsync(otel =>
            {
                otel.UseArcusObservability(options =>
                {
                    if (componentName != null)
                    {
                        options.UseAppName(componentName);
                    }

                    options.UseAssemblyAppVersion<OpenTelemetryObservabilityOperationTests>();
                });
            });

            // Act
            context.WhenObservability(obs =>
            {
                using var req = obs.StartCustomDependency(dependencyName);
                req.IsSuccessful = isSuccessful;
            });

            // Assert
            context.ShouldFindSingleOperation(dependencyName, isSuccessful, componentName);
        }

        private sealed class OpenTelemetryRequestTestContext : IAsyncDisposable
        {
            private readonly IHost _host;
            private readonly Collection<Activity> _actualActivities;

            private OpenTelemetryRequestTestContext(IHost host, Collection<Activity> actualActivities)
            {
                _host = host;
                _actualActivities = actualActivities;
            }

            public static async Task<OpenTelemetryRequestTestContext> GivenAsync(Action<OpenTelemetryBuilder> configureRequests)
            {
                var builder = Host.CreateDefaultBuilder();

                var actualRequests = new Collection<Activity>();
                builder.ConfigureServices(services =>
                {
                    configureRequests(
                        services.AddOpenTelemetry()
                                .WithTracing(traces => traces.AddInMemoryExporter(actualRequests)));
                });

                var host = builder.Build();
                await host.StartAsync(TestContext.Current.CancellationToken);

                return new(host, actualRequests);
            }

            public void WhenObservability(Action<IObservability> act)
            {
                act(_host.Services.GetRequiredService<IObservability>());
            }

            public void ShouldFindSingleOperation(string operationName, bool isSuccessful, string componentName)
            {
                _host.Services.GetRequiredService<TracerProvider>().ForceFlush();

                Activity actual = Assert.Single(_actualActivities, m => m.OperationName == operationName);
                Assert.Equal(isSuccessful, actual.Status is ActivityStatusCode.Ok);
                if (componentName != null)
                {
                    Assert.Equal(componentName, actual.Source.Name);
                }
            }

            public async ValueTask DisposeAsync()
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
    }
}
