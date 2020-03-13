﻿using Arcus.Observability.Telemetry.Serilog.Enrichers;
using GuardNet;
using Serilog.Configuration;

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
    }
}
