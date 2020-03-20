using System;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using GuardNet;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

// ReSharper disable once CheckNamespace
namespace Serilog
{
    /// <summary>
    /// Adds user-friendly extensions to append additional Serilog enrichers to the <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Adds the <see cref="VersionEnricher"/> to the logger enrichment configuration which adds the current runtime version (i.e. 'version' = '1.0.0').
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        public static LoggerConfiguration WithVersion(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

            return enrichmentConfiguration.With<VersionEnricher>();
        }
        
        /// <summary>
        /// Adds the <see cref="ApplicationEnricher"/> to the logger enrichment configuration which adds the given application's <paramref name="componentName"/>.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="componentName">The name of the application component.</param>
        public static LoggerConfiguration WithComponentName(this LoggerEnrichmentConfiguration enrichmentConfiguration, string componentName)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration));
            Guard.NotNullOrWhitespace(componentName, nameof(componentName), "Application component name cannot be blank");

            return enrichmentConfiguration.With(new ApplicationEnricher(componentName));
        }

        /// <summary>
        /// Adds the <see cref="KubernetesEnricher"/> to the logger enrichment configuration which adds Kubernetes information from the environment.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        public static LoggerConfiguration WithKubernetesInfo(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

            return enrichmentConfiguration.With<KubernetesEnricher>();
        }

        /// <summary>
        /// Adds the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> to the logger enrichment configuration which adds the <see cref="CorrelationInfo"/> information from the current context.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="correlationInfoAccessor">The accessor implementation for the <see cref="CorrelationInfo"/> model.</param>
        public static LoggerConfiguration WithCorrelationInfo(this LoggerEnrichmentConfiguration enrichmentConfiguration, ICorrelationInfoAccessor correlationInfoAccessor)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration));
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor));

            return WithCorrelationInfo<CorrelationInfo>(enrichmentConfiguration, correlationInfoAccessor);
        }

        /// <summary>
        /// Adds the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> to the logger enrichment configuration which adds the custom <see cref="CorrelationInfo"/> information from the current context.
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="correlationInfoAccessor">The accessor implementation for the <see cref="CorrelationInfo"/> model.</param>
        public static LoggerConfiguration WithCorrelationInfo<TCorrelationInfo>(
            this LoggerEnrichmentConfiguration enrichmentConfiguration, 
            ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor) where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration));
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor));

            return enrichmentConfiguration.With(new CorrelationInfoEnricher<TCorrelationInfo>(correlationInfoAccessor));
        }
    }
}
