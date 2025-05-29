using System;
using Arcus.Observability;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights;
using GuardNet;
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
            Guard.NotNull(builder, nameof(builder), "Requires a logging builder instance to add the Serilog logger provider");
            Guard.NotNull(implementationFactory, nameof(implementationFactory), "Requires an implementation factory to build up the Serilog logger");

            builder.Services.AddSingleton<ILoggerProvider>(provider =>
            {
                return new SerilogLoggerProvider(implementationFactory(provider), dispose: true);
            });

            return builder;
        }

        /// <summary>
        /// Adds an <see cref="ITelemetryLogger{TCategoryName}"/> implementation to the application services that interacts via Serilog to Azure Application Insights.
        /// This allows additional telemetry to be written to Application Insights, such as custom events, metrics, requests, dependencies, etc.
        /// </summary>
        public static ILoggingBuilder UseCustomSerilogTelemetry(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ITelemetryLogger<>), typeof(SerilogTelemetryLogger<>));
            return builder;
        }
    }
}
