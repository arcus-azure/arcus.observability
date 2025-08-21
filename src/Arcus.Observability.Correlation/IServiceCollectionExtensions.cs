using System;
using Arcus.Observability.Correlation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to set correlation access information on the <see cref="IServiceCollection"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds operation and transaction correlation to the application using the <see cref="DefaultCorrelationInfoAccessor"/>
        /// </summary>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddCorrelation(this IServiceCollection services)
        {
            return AddCorrelation<CorrelationInfo>(services);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application using the <see cref="DefaultCorrelationInfoAccessor{TCorrelationInfo}"/>
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddCorrelation<TCorrelationInfo>(
            this IServiceCollection services)
            where TCorrelationInfo : CorrelationInfo
        {
            return AddCorrelation<DefaultCorrelationInfoAccessor<TCorrelationInfo>, TCorrelationInfo>(
                services,
                provider => new DefaultCorrelationInfoAccessor<TCorrelationInfo>());
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> or the <paramref name="createCustomCorrelationAccessor"/> is <c>null</c>.</exception>
        public static IServiceCollection AddCorrelation<TAccessor>(
            this IServiceCollection services,
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor)
            where TAccessor : class, ICorrelationInfoAccessor
        {
            return AddCorrelation<TAccessor, CorrelationInfo>(services, createCustomCorrelationAccessor);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> or the <paramref name="createCustomCorrelationAccessor"/> is <c>null</c>.</exception>
        public static IServiceCollection AddCorrelation<TAccessor, TCorrelationInfo>(
            this IServiceCollection services,
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor)
            where TAccessor : class, ICorrelationInfoAccessor<TCorrelationInfo>
            where TCorrelationInfo : CorrelationInfo
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(createCustomCorrelationAccessor);

            services.AddScoped<ICorrelationInfoAccessor<TCorrelationInfo>>(createCustomCorrelationAccessor);
            services.AddScoped<ICorrelationInfoAccessor>(serviceProvider =>
            {
                return new CorrelationInfoAccessorProxy<TCorrelationInfo>(
                    serviceProvider.GetRequiredService<ICorrelationInfoAccessor<TCorrelationInfo>>());
            });

            return services;
        }
    }
}
