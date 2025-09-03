using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions on the <see cref="ILoggingBuilder"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggingBuilderExtensions
    {
        /// <summary>
        /// Add Serilog to the logging pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder" /> to add logging provider to.</param>
        /// <param name="implementationFactory">The factory function to create an Serilog logger implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="implementationFactory"/> is <c>null</c>.</exception>
        public static ILoggingBuilder AddSerilog(
            this ILoggingBuilder builder,
            Func<IServiceProvider, Serilog.ILogger> implementationFactory)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(implementationFactory);

            builder.Services.AddSingleton<ILoggerProvider>(provider =>
            {
                return new SerilogLoggerProvider(implementationFactory(provider), dispose: true);
            });

            return builder;
        }
    }
}
