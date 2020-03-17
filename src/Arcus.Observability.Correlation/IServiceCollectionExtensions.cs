using System;
using Arcus.Observability.Correlation;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions to set correlation access information on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        public static IServiceCollection AddCorrelation(
            this IServiceCollection services,
            Action<CorrelationInfoOptions> configureOptions = null)
        {
            Guard.NotNull(services, nameof(services));

            return AddCorrelation(services, new DefaultCorrelationInfoAccessor(), configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="customCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        public static IServiceCollection AddCorrelation<TAccessor>(
            this IServiceCollection services,
            TAccessor customCorrelationAccessor,
            Action<CorrelationInfoOptions> configureOptions = null)
            where TAccessor : class, ICorrelationInfoAccessor
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(customCorrelationAccessor, nameof(customCorrelationAccessor));

            return AddCorrelation(services, serviceProvider => customCorrelationAccessor, configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        public static IServiceCollection AddCorrelation<TAccessor>(
            this IServiceCollection services, 
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor,
            Action<CorrelationInfoOptions> configureOptions = null)
            where TAccessor : class, ICorrelationInfoAccessor
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(createCustomCorrelationAccessor, nameof(createCustomCorrelationAccessor));

            return AddCorrelation<TAccessor, CorrelationInfo>(services, createCustomCorrelationAccessor, configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <typeparam name="TCorrelationInfo">The type of the <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        public static IServiceCollection AddCorrelation<TAccessor, TCorrelationInfo>(
            this IServiceCollection services,
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor,
            Action<CorrelationInfoOptions> configureOptions = null)
            where TAccessor : class, ICorrelationInfoAccessor<TCorrelationInfo> 
            where TCorrelationInfo : CorrelationInfo 
        {
            Guard.NotNull(services, nameof(services));

            services.AddSingleton<ICorrelationInfoAccessor<TCorrelationInfo>>(createCustomCorrelationAccessor);
            services.AddSingleton<ICorrelationInfoAccessor>(serviceProvider =>
            {
                return new CorrelationInfoAccessorProxy<TCorrelationInfo>(
                    serviceProvider.GetRequiredService<ICorrelationInfoAccessor<TCorrelationInfo>>());
            });

            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            return services;
        }
    }
}