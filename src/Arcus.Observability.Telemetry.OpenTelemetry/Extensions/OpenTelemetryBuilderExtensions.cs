using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Arcus.Observability;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

#pragma warning disable IDE0130 // Extensions should be in the same namespace as the extended type.
namespace OpenTelemetry
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents the available options to configure the OpenTelemetry <see cref="IObservability"/> implementation
    /// via <see cref="OpenTelemetryBuilderExtensions.UseArcusObservability(OpenTelemetryBuilder,Action{OpenTelemetryObservabilityOptions})"/>
    /// </summary>
    public class OpenTelemetryObservabilityOptions
    {
        internal Func<IServiceProvider, IAppName> AppNameImplementationFactory { get; set; }
        internal Func<IServiceProvider, IAppVersion> AppVersionImplementationFactory { get; set; }

        internal static string GetDefaultMeterName()
        {
            return Assembly.GetEntryAssembly()?.GetName().Name ?? "Default.Meter";
        }

        /// <summary>
        /// Uses the <see cref="IAppName"/> to configure the OpenTelemetry service name in the application.
        /// </summary>
        /// <param name="componentName">The functional name to identity the application.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="componentName"/> is blank.</exception>
        public OpenTelemetryObservabilityOptions UseAppName(string componentName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(componentName);
            return UseAppName(_ => new DefaultAppName(componentName));
        }

        /// <summary>
        /// Uses the <see cref="IAppName"/> to configure the OpenTelemetry service name in the application.
        /// </summary>
        /// <param name="implementationFactory">The factory function to create the <see cref="IAppName"/> implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="implementationFactory"/> is <c>null</c>.</exception>
        public OpenTelemetryObservabilityOptions UseAppName(Func<IServiceProvider, IAppName> implementationFactory)
        {
            ArgumentNullException.ThrowIfNull(implementationFactory);
            AppNameImplementationFactory = implementationFactory;

            return this;
        }

        /// <summary>
        /// Uses the <see cref="IAppVersion"/> to configure the OpenTelemetry service version in the application.
        /// </summary>
        /// <typeparam name="TConsumerType">The type of the consumer project to determine the version based on the assembly version.</typeparam>
        public OpenTelemetryObservabilityOptions UseAssemblyAppVersion<TConsumerType>()
        {
            return UseAppVersion(_ => new AssemblyAppVersion(typeof(TConsumerType)));
        }

        /// <summary>
        /// Uses the <see cref="IAppVersion"/> to configure the OpenTelemetry service version in the application.
        /// </summary>
        /// <param name="implementationFactory">The factory function to create the <see cref="IAppVersion"/> implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="implementationFactory"/> is <c>null</c>.</exception>
        public OpenTelemetryObservabilityOptions UseAppVersion(Func<IServiceProvider, IAppVersion> implementationFactory)
        {
            ArgumentNullException.ThrowIfNull(implementationFactory);
            AppVersionImplementationFactory = implementationFactory;

            return this;
        }
    }

    /// <summary>
    /// Extensions on the <see cref="OpenTelemetryBuilder"/> to add dev-friendly telemetry throughout the application.
    /// </summary>
    public static class OpenTelemetryBuilderExtensions
    {
        /// <summary>
        /// Registers an <see cref="IObservability"/> implementation based on the OpenTelemetry setup.
        /// </summary>
        /// <param name="otel">The current OpenTelemetry setup being configured in the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="otel"/> is <c>null</c>.</exception>
        public static OpenTelemetryBuilder UseArcusObservability(this OpenTelemetryBuilder otel)
        {
            return UseArcusObservability(otel, configureOptions: null);
        }

        /// <summary>
        /// Registers an <see cref="IObservability"/> implementation based on the OpenTelemetry setup.
        /// </summary>
        /// <param name="otel">The current OpenTelemetry setup being configured in the application.</param>
        /// <param name="configureOptions">The additional options to manipulate the behavior of the <see cref="IObservability"/> implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="otel"/> is <c>null</c>.</exception>
        public static OpenTelemetryBuilder UseArcusObservability(this OpenTelemetryBuilder otel, Action<OpenTelemetryObservabilityOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(otel);

            var options = new OpenTelemetryObservabilityOptions();
            configureOptions?.Invoke(options);

            if (options.AppNameImplementationFactory != null)
            {
                otel.Services.TryAddSingleton(options.AppNameImplementationFactory);
            }

            if (options.AppVersionImplementationFactory != null)
            {
                otel.Services.TryAddSingleton(options.AppVersionImplementationFactory);
            }

            otel.Services.TryAddSingleton(provider =>
            {
                var appName = provider.GetService<IAppName>();
                string componentName = appName is null
                    ? Assembly.GetEntryAssembly()?.GetName().Name ?? "default.app"
                    : appName.GetApplicationName();

                return new ActivitySource(componentName);
            });
            otel.Services.TryAddSingleton<IObservability>(provider =>
            {
                var source = provider.GetRequiredService<ActivitySource>();
                var factory = provider.GetRequiredService<IMeterFactory>();

                return new OpenTelemetryObservability(source, factory, options);
            });

            otel.ConfigureResource(resource =>
            {
                resource.AddDetector(provider =>
                {
                    return new AppNameResourceDetector(
                        provider.GetService<IAppName>(),
                        provider.GetService<IAppVersion>());
                });
            });
            otel.Services.ConfigureOpenTelemetryTracerProvider((provider, traces) =>
            {
                var source = provider.GetRequiredService<ActivitySource>();
                traces.AddSource(source.Name);
            });
            otel.WithMetrics(metrics =>
            {
                metrics.AddMeter(OpenTelemetryObservabilityOptions.GetDefaultMeterName());
            });

            return otel;
        }

        private sealed class AppNameResourceDetector : IResourceDetector
        {
            private readonly IAppName _appName;
            private readonly IAppVersion _appVersion;

            /// <summary>
            /// Initializes a new instance of the <see cref="AppNameResourceDetector"/> class.
            /// </summary>
            public AppNameResourceDetector(IAppName appName, IAppVersion appVersion)
            {
                _appName = appName;
                _appVersion = appVersion;
            }

            /// <summary>
            /// Called to get a resource with attributes from detector.
            /// </summary>
            /// <returns>An instance of <see cref="Resource" />.</returns>
            public Resource Detect()
            {
                var attributes = new Collection<KeyValuePair<string, object>>();
                if (_appName != null)
                {
                    attributes.Add(new KeyValuePair<string, object>("service.name", _appName.GetApplicationName()));
                }

                if (_appVersion != null)
                {
                    attributes.Add(new KeyValuePair<string, object>("service.version", _appVersion.GetVersion()));
                }

                return new Resource(attributes);
            }
        }
    }
}
