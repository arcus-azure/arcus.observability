using System;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enriches the log events with the correlation information.
    /// </summary>
    public class CorrelationInfoEnricher<TCorrelationInfo> : ILogEventEnricher where TCorrelationInfo : CorrelationInfo
    {
        private readonly string _operationIdPropertyName, _transactionIdPropertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> class.
        /// </summary>
        /// <param name="correlationInfoAccessor">The accessor implementation for the custom <see cref="CorrelationInfo"/> model.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="correlationInfoAccessor"/> is <c>null</c>.</exception>
        public CorrelationInfoEnricher(ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor)
            : this(correlationInfoAccessor, ContextProperties.Correlation.OperationId, ContextProperties.Correlation.TransactionId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> class.
        /// </summary>
        /// <param name="correlationInfoAccessor">The accessor implementation for the custom <see cref="CorrelationInfo"/> model.</param>
        /// <param name="operationIdPropertyName">The name of the property to enrich the log event with the correlation operation ID.</param>
        /// <param name="transactionIdPropertyName">The name of the property to enrich the log event with the correlation transaction ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="correlationInfoAccessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationIdPropertyName"/> or <paramref name="transactionIdPropertyName"/> is blank.</exception>
        public CorrelationInfoEnricher(
            ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor, 
            string operationIdPropertyName,
            string transactionIdPropertyName)
        {
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor), "Requires an correlation accessor to enrich the log events with correlation information");
            Guard.NotNullOrWhitespace(operationIdPropertyName, nameof(operationIdPropertyName), "Requires a property name to enrich the log event with the correlation operation ID");
            Guard.NotNullOrWhitespace(transactionIdPropertyName, nameof(transactionIdPropertyName), "Requires a property name to enrich the log event with the correlation transaction ID");

            _operationIdPropertyName = operationIdPropertyName;
            _transactionIdPropertyName = transactionIdPropertyName;

            CorrelationInfoAccessor = correlationInfoAccessor;
        }

        /// <summary>
        /// Gets the <see cref="ICorrelationInfoAccessor{TCorrelationInfo}"/> that provides the correlation model.
        /// </summary>
        protected ICorrelationInfoAccessor<TCorrelationInfo> CorrelationInfoAccessor { get; }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            Guard.NotNull(logEvent, nameof(logEvent));
            Guard.NotNull(propertyFactory, nameof(propertyFactory));

            TCorrelationInfo correlationInfo = CorrelationInfoAccessor.GetCorrelationInfo();
            if (correlationInfo is null)
            {
                return;
            }

            EnrichCorrelationInfo(logEvent, propertyFactory, correlationInfo);
        }

        /// <summary>
        /// Enrich the <paramref name="logEvent"/> with the given <paramref name="correlationInfo"/> model.
        /// </summary>
        /// <param name="logEvent">The log event to enrich with correlation information.</param>
        /// <param name="propertyFactory">The log property factory to create log properties with correlation information.</param>
        /// <param name="correlationInfo">The correlation model that contains the current correlation information.</param>
        protected virtual void EnrichCorrelationInfo(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TCorrelationInfo correlationInfo)
        {
            if (!String.IsNullOrEmpty(correlationInfo.OperationId))
            {
                LogEventProperty property = propertyFactory.CreateProperty(_operationIdPropertyName, correlationInfo.OperationId);
                logEvent.AddPropertyIfAbsent(property);
            }

            if (!String.IsNullOrEmpty(correlationInfo.TransactionId))
            {
                LogEventProperty property = propertyFactory.CreateProperty(_transactionIdPropertyName, correlationInfo.TransactionId);
                logEvent.AddPropertyIfAbsent(property);
            }
        }
    }
}
