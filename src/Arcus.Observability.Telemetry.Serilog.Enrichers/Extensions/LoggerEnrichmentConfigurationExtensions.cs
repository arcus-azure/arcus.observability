using System;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
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
        /// <param name="propertyName">The name of the property to enrich the log event with the current runtime version.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyName"/> is blank.</exception>
        public static LoggerConfiguration WithVersion(this LoggerEnrichmentConfiguration enrichmentConfiguration, string propertyName = VersionEnricher.DefaultPropertyName)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the version enricher");
            Guard.NotNullOrWhitespace(propertyName, nameof(propertyName), "Requires a non-blank property name to enrich the log event with the current runtime version");

            return enrichmentConfiguration.With(new VersionEnricher(propertyName));
        }

        /// <summary>
        /// Adds the <see cref="ApplicationEnricher"/> to the logger enrichment configuration which adds the given application's <paramref name="componentName"/>.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="componentName">The name of the application component.</param>
        /// <param name="propertyName">The name of the property to enrich the log event with the <paramref name="componentName"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="componentName"/> or <paramref name="propertyName"/> is blank.</exception>
        public static LoggerConfiguration WithComponentName(
            this LoggerEnrichmentConfiguration enrichmentConfiguration, 
            string componentName, 
            string propertyName = ApplicationEnricher.ComponentName)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Require an enrichment configuration to add the application component enricher");
            Guard.NotNullOrWhitespace(componentName, nameof(componentName), "Requires a non-blank application component name");
            Guard.NotNullOrWhitespace(propertyName, nameof(propertyName), "Requires a non-blank property name to enrich the log event with the component name");

            return enrichmentConfiguration.With(new ApplicationEnricher(componentName, propertyName));
        }

        /// <summary>
        /// Adds the <see cref="KubernetesEnricher"/> to the logger enrichment configuration which adds Kubernetes information from the environment.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="nodeNamePropertyName">The name of the property to enrich the log event with the Kubernetes node name.</param>
        /// <param name="podNamePropertyName">The name of the property to enrich the log event with the Kubernetes pod name.</param>
        /// <param name="namespacePropertyName">The name of the property to enrich the log event with the Kubernetes namespace.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="nodeNamePropertyName"/>, <paramref name="podNamePropertyName"/>, or <paramref name="namespacePropertyName"/> is blank.</exception>
        public static LoggerConfiguration WithKubernetesInfo(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            string nodeNamePropertyName = ContextProperties.Kubernetes.NodeName,
            string podNamePropertyName = ContextProperties.Kubernetes.PodName,
            string namespacePropertyName = ContextProperties.Kubernetes.Namespace)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the Kubernetes enricher");
            Guard.NotNullOrWhitespace(nodeNamePropertyName, nameof(nodeNamePropertyName), "Requires a non-blank property name to enrich the log event with the Kubernetes node name");
            Guard.NotNullOrWhitespace(podNamePropertyName, nameof(podNamePropertyName), "Requires a non-blank property name to enrich the log event with the Kubernetes pod name");
            Guard.NotNullOrWhitespace(namespacePropertyName, nameof(namespacePropertyName), "Requires a non-blank property name to enrich the log event with the Kubernetes namespace name");

            return enrichmentConfiguration.With(new KubernetesEnricher(nodeNamePropertyName, podNamePropertyName, namespacePropertyName));
        }

        /// <summary>
        /// Adds the <see cref="DefaultCorrelationInfoAccessor"/> to the logger enrichment configuration which adds the <see cref="CorrelationInfo"/> information from the current context.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="operationIdPropertyName">The name of the property to enrich the log event with the correlation operation ID.</param>
        /// <param name="transactionIdPropertyName">The name of the property to enrich the log event with the correlation transaction ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationIdPropertyName"/> or <paramref name="transactionIdPropertyName"/> is blank.</exception>
        public static LoggerConfiguration WithCorrelationInfo(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            string operationIdPropertyName = ContextProperties.Correlation.OperationId,
            string transactionIdPropertyName = ContextProperties.Correlation.TransactionId)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the correlation information enricher");
            Guard.NotNullOrWhitespace(operationIdPropertyName, nameof(operationIdPropertyName), "Requires a property name to enrich the log event with the correlation operation ID");
            Guard.NotNullOrWhitespace(transactionIdPropertyName, nameof(transactionIdPropertyName), "Requires a property name to enrich the log event with the correlation transaction ID");

            return WithCorrelationInfo(enrichmentConfiguration, DefaultCorrelationInfoAccessor.Instance, operationIdPropertyName, transactionIdPropertyName);
        }

        /// <summary>
        /// Adds the <see cref="DefaultCorrelationInfoAccessor{TCorrelationInfo}"/> to the logger enrichment configuration which adds the <see cref="CorrelationInfo"/> information from the current context.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="operationIdPropertyName">The name of the property to enrich the log event with the correlation operation ID.</param>
        /// <param name="transactionIdPropertyName">The name of the property to enrich the log event with the correlation transaction ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationIdPropertyName"/> or <paramref name="transactionIdPropertyName"/> is blank.</exception>
        public static LoggerConfiguration WithCorrelationInfo<TCorrelationInfo>(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            string operationIdPropertyName = ContextProperties.Correlation.OperationId,
            string transactionIdPropertyName = ContextProperties.Correlation.TransactionId)
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the correlation information enricher");
            Guard.NotNullOrWhitespace(operationIdPropertyName, nameof(operationIdPropertyName), "Requires a property name to enrich the log event with the correlation operation ID");
            Guard.NotNullOrWhitespace(transactionIdPropertyName, nameof(transactionIdPropertyName), "Requires a property name to enrich the log event with the correlation transaction ID");

            return WithCorrelationInfo(enrichmentConfiguration, DefaultCorrelationInfoAccessor<TCorrelationInfo>.Instance, operationIdPropertyName, transactionIdPropertyName);
        }

        /// <summary>
        /// Adds the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> to the logger enrichment configuration which adds the <see cref="CorrelationInfo"/> information from the current context.
        /// </summary>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="correlationInfoAccessor">The accessor implementation for the <see cref="CorrelationInfo"/> model.</param>
        /// <param name="operationIdPropertyName">The name of the property to enrich the log event with the correlation operation ID.</param>
        /// <param name="transactionIdPropertyName">The name of the property to enrich the log event with the correlation transaction ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> or <paramref name="correlationInfoAccessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationIdPropertyName"/> or <paramref name="transactionIdPropertyName"/> is blank.</exception>
        public static LoggerConfiguration WithCorrelationInfo(
            this LoggerEnrichmentConfiguration enrichmentConfiguration, 
            ICorrelationInfoAccessor correlationInfoAccessor,
            string operationIdPropertyName = ContextProperties.Correlation.OperationId,
            string transactionIdPropertyName = ContextProperties.Correlation.TransactionId)
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the correlation information enricher");
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor), "Requires an correlation accessor to retrieve the correlation information during the enrichment of the log events");
            Guard.NotNullOrWhitespace(operationIdPropertyName, nameof(operationIdPropertyName), "Requires a property name to enrich the log event with the correlation operation ID");
            Guard.NotNullOrWhitespace(transactionIdPropertyName, nameof(transactionIdPropertyName), "Requires a property name to enrich the log event with the correlation transaction ID");

            return WithCorrelationInfo<CorrelationInfo>(enrichmentConfiguration, correlationInfoAccessor, operationIdPropertyName, transactionIdPropertyName);
        }

        /// <summary>
        /// Adds the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> to the logger enrichment configuration which adds the custom <typeparamref name="TCorrelationInfo"/> information from the current context.
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="correlationInfoAccessor">The accessor implementation for the <typeparamref name="TCorrelationInfo"/> model.</param>
        /// <param name="operationIdPropertyName">The name of the property to enrich the log event with the correlation operation ID.</param>
        /// <param name="transactionIdPropertyName">The name of the property to enrich the log event with the correlation transaction ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> or <paramref name="correlationInfoAccessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationIdPropertyName"/> or <paramref name="transactionIdPropertyName"/> is blank.</exception>
        public static LoggerConfiguration WithCorrelationInfo<TCorrelationInfo>(
            this LoggerEnrichmentConfiguration enrichmentConfiguration, 
            ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor,
            string operationIdPropertyName = ContextProperties.Correlation.OperationId,
            string transactionIdPropertyName = ContextProperties.Correlation.TransactionId) 
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the correlation information enricher");
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor), "Requires an correlation accessor to retrieve the correlation information during the enrichment of the log events");
            Guard.NotNullOrWhitespace(operationIdPropertyName, nameof(operationIdPropertyName), "Requires a property name to enrich the log event with the correlation operation ID");
            Guard.NotNullOrWhitespace(transactionIdPropertyName, nameof(transactionIdPropertyName), "Requires a property name to enrich the log event with the correlation transaction ID");

            return enrichmentConfiguration.With(new CorrelationInfoEnricher<TCorrelationInfo>(correlationInfoAccessor, operationIdPropertyName, transactionIdPropertyName));
        }

        /// <summary>
        /// Adds the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> to the logger enrichment configuration which adds the custom <typeparamref name="TCorrelationInfo"/> information from the current context.
        /// </summary>
        /// <typeparam name="TCorrelationInfo">The type of the custom <see cref="CorrelationInfo"/> model.</typeparam>
        /// <param name="enrichmentConfiguration">The configuration to add the enricher.</param>
        /// <param name="correlationInfoEnricher">The custom correlation enricher implementation for the <typeparamref name="TCorrelationInfo"/> model.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="enrichmentConfiguration"/> or the <paramref name="correlationInfoEnricher"/> is <c>null</c>.</exception>
        public static LoggerConfiguration WithCorrelationInfo<TCorrelationInfo>(
            this LoggerEnrichmentConfiguration enrichmentConfiguration, 
            CorrelationInfoEnricher<TCorrelationInfo> correlationInfoEnricher) 
            where TCorrelationInfo : CorrelationInfo
        {
            Guard.NotNull(enrichmentConfiguration, nameof(enrichmentConfiguration), "Requires an enrichment configuration to add the correlation information enricher");
            Guard.NotNull(correlationInfoEnricher, nameof(correlationInfoEnricher), "Requires an correlation enricher to enrich the log events with correlation information");

            return enrichmentConfiguration.With(correlationInfoEnricher);
        }
    }
}
