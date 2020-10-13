using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="IAppVersion"/> implementation to the application which can be used to retrieve the current application's version.
        /// </summary>
        /// <typeparam name="TAppVersion">The type that implements the <see cref="IAppVersion"/> interface.</typeparam>
        /// <param name="services">The collection of registered services to add the <see cref="IAppVersion"/> implementation to.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddAppVersion<TAppVersion>(this IServiceCollection services) where TAppVersion : class, IAppVersion
        {
            Guard.NotNull(services, nameof(services), $"Requires a collection of services to add the '{nameof(IAppVersion)}' implementation");
            return services.AddSingleton<IAppVersion, TAppVersion>();
        }

        /// <summary>
        /// Adds an <see cref="IAppVersion"/> implementation to the application which can be used to retrieve the current application's version.
        /// </summary>
        /// <param name="services">The collection of registered services to add the <see cref="IAppVersion"/> implementation to.</param>
        /// <param name="createImplementation">The factory function to create the <see cref="IAppVersion"/> implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> or the <paramref name="createImplementation"/> is <c>null</c>.</exception>
        public static IServiceCollection AddAppVersion(
            this IServiceCollection services,
            Func<IServiceProvider, IAppVersion> createImplementation)
        {
            Guard.NotNull(services, nameof(services), $"Requires a collection of services to add the '{nameof(IAppVersion)}' implementation");
            Guard.NotNull(createImplementation, nameof(createImplementation), $"Requires a factory function to create the '{nameof(IAppVersion)}' implementation");

            return services.AddSingleton(createImplementation);
        }
    }
}
