using System;
using Arcus.Observability.Correlation;
using GuardNet;

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
        public static IServiceCollection AddCorrelation(this IServiceCollection services)
        {
            Guard.NotNull(services, nameof(services));

            return AddCorrelation<CorrelationInfo>(services);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application using the <see cref="DefaultCorrelationInfoAccessor"/>
        /// </summary>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
        public static IServiceCollection AddCorrelation(
            this IServiceCollection services,
            Action<CorrelationInfoOptions> configureOptions)
        {
            Guard.NotNull(services, nameof(services));

            return AddCorrelation(services, new DefaultCorrelationInfoAccessor(), configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application using the <see cref="DefaultCorrelationInfoAccessor{TCorrelationInfo}"/>
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        public static IServiceCollection AddCorrelation<TCorrelationInfo>(
            this IServiceCollection services) 
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(services, nameof(services));

            return AddCorrelation<DefaultCorrelationInfoAccessor<TCorrelationInfo>, TCorrelationInfo>(
                services, 
                provider => new DefaultCorrelationInfoAccessor<TCorrelationInfo>());
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application using the <see cref="DefaultCorrelationInfoAccessor{TCorrelationInfo}"/>
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
        public static IServiceCollection AddCorrelation<TCorrelationInfo>(
            this IServiceCollection services,
            Action<CorrelationInfoOptions> configureOptions) 
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(services, nameof(services));

            return AddCorrelation<TCorrelationInfo, CorrelationInfoOptions>(services, configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application using the <see cref="DefaultCorrelationInfoAccessor{TCorrelationInfo}"/>.
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the <see cref="CorrelationInfo"/> model.</typeparam>
        /// <typeparam name="TOptions">The type of the custom <see cref="CorrelationInfoOptions"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
        public static IServiceCollection AddCorrelation<TCorrelationInfo, TOptions>(
            this IServiceCollection services,
            Action<TOptions> configureOptions = null)
            where TCorrelationInfo : CorrelationInfo
            where TOptions : CorrelationInfoOptions
        {
            Guard.NotNull(services, nameof(services));

            return AddCorrelation<DefaultCorrelationInfoAccessor<TCorrelationInfo>, TCorrelationInfo, TOptions>(
                services, 
                serviceProvider => new DefaultCorrelationInfoAccessor<TCorrelationInfo>(),
                configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="customCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
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
        public static IServiceCollection AddCorrelation<TAccessor>(
            this IServiceCollection services, 
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor)
            where TAccessor : class, ICorrelationInfoAccessor
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(createCustomCorrelationAccessor, nameof(createCustomCorrelationAccessor));

            return AddCorrelation<TAccessor, CorrelationInfo>(services, createCustomCorrelationAccessor);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
        public static IServiceCollection AddCorrelation<TAccessor>(
            this IServiceCollection services, 
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor,
            Action<CorrelationInfoOptions> configureOptions)
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
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        public static IServiceCollection AddCorrelation<TAccessor, TCorrelationInfo>(
            this IServiceCollection services,
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor)
            where TAccessor : class, ICorrelationInfoAccessor<TCorrelationInfo>
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(createCustomCorrelationAccessor, nameof(createCustomCorrelationAccessor));

            services.AddScoped<ICorrelationInfoAccessor<TCorrelationInfo>>(createCustomCorrelationAccessor);
            services.AddScoped<ICorrelationInfoAccessor>(serviceProvider =>
            {
                return new CorrelationInfoAccessorProxy<TCorrelationInfo>(
                    serviceProvider.GetRequiredService<ICorrelationInfoAccessor<TCorrelationInfo>>());
            });

            return services;
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
        public static IServiceCollection AddCorrelation<TAccessor, TCorrelationInfo>(
            this IServiceCollection services,
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor,
            Action<CorrelationInfoOptions> configureOptions)
            where TAccessor : class, ICorrelationInfoAccessor<TCorrelationInfo>
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(createCustomCorrelationAccessor, nameof(createCustomCorrelationAccessor));

            return AddCorrelation<TAccessor, TCorrelationInfo, CorrelationInfoOptions>(services, createCustomCorrelationAccessor, configureOptions);
        }

        /// <summary>
        /// Adds operation and transaction correlation to the application.
        /// </summary>
        /// <typeparam name="TAccessor">The type of the <see cref="ICorrelationInfoAccessor"/> implementation.</typeparam>
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <typeparam name="TOptions">The type of the custom <see cref="CorrelationInfoOptions"/> model.</typeparam>
        /// <param name="services">The services collection containing the dependency injection services.</param>
        /// <param name="createCustomCorrelationAccessor">The custom <see cref="ICorrelationInfoAccessor"/> implementation factory to retrieve the <see cref="CorrelationInfo"/>.</param>
        /// <param name="configureOptions">The function to configure additional options how the correlation works.</param>
        [Obsolete("Use HTTP or messaging-specific correlation registration instead")]
        public static IServiceCollection AddCorrelation<TAccessor, TCorrelationInfo, TOptions>(
            this IServiceCollection services,
            Func<IServiceProvider, TAccessor> createCustomCorrelationAccessor,
            Action<TOptions> configureOptions)
            where TAccessor : class, ICorrelationInfoAccessor<TCorrelationInfo> 
            where TCorrelationInfo : CorrelationInfo
            where TOptions : CorrelationInfoOptions
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(createCustomCorrelationAccessor, nameof(createCustomCorrelationAccessor));

            services.AddScoped<ICorrelationInfoAccessor<TCorrelationInfo>>(createCustomCorrelationAccessor);
            services.AddScoped<ICorrelationInfoAccessor>(serviceProvider =>
            {
                return new CorrelationInfoAccessorProxy<TCorrelationInfo>(
                    serviceProvider.GetRequiredService<ICorrelationInfoAccessor<TCorrelationInfo>>());
            });

            services.Configure<TOptions>(options => configureOptions?.Invoke(options));

            return services;
        }
    }
}