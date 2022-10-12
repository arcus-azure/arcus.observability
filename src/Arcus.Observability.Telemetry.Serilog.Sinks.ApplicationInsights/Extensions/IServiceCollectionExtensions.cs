using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using GuardNet;
using Microsoft.ApplicationInsights.Extensibility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/> related to Application Insights.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
         /// <summary>
        /// Adds an <see cref="IAppName"/> implementation to the application which can be used to retrieve the current application's name.
        /// </summary>
        /// <param name="services">The collection of registered services to add the <see cref="IAppName"/> implementation to.</param>
        /// <param name="componentName">The functional name to identity the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="componentName"/> is blank.</exception>
        public static IServiceCollection AddAppName(
            this IServiceCollection services,
            string componentName)
        {
            Guard.NotNull(services, nameof(services), $"Requires a collection of services to add the '{nameof(IAppName)}' implementation");
            Guard.NotNullOrWhitespace(componentName, nameof(componentName), "Requires a non-blank functional name to identity the application");

            return AddAppName(services, provider => new DefaultAppName(componentName));
        }

        /// <summary>
        /// Adds an <see cref="IAppName"/> implementation to the application which can be used to retrieve the current application's name.
        /// </summary>
        /// <param name="services">The collection of registered services to add the <see cref="IAppName"/> implementation to.</param>
        /// <param name="implementationFactory">The factory function to create the <see cref="IAppName"/> implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> or the <paramref name="implementationFactory"/> is <c>null</c>.</exception>
        public static IServiceCollection AddAppName(
            this IServiceCollection services,
            Func<IServiceProvider, IAppName> implementationFactory)
        {
            Guard.NotNull(services, nameof(services), $"Requires a collection of services to add the '{nameof(IAppName)}' implementation");
            Guard.NotNull(implementationFactory, nameof(implementationFactory), $"Requires a factory function to create the '{nameof(IAppName)}' implementation");

            return services.AddSingleton(implementationFactory)
                           .AddSingleton<ITelemetryInitializer, ApplicationTelemetryInitializer>();
        }
    }
}
