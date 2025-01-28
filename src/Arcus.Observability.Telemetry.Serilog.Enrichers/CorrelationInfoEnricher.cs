using System;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers
{
    /// <summary>
    /// Enriches the log events with the correlation information.
    /// </summary>
    public class CorrelationInfoEnricher<TCorrelationInfo> : ILogEventEnricher where TCorrelationInfo : CorrelationInfo
    {
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
            : this(correlationInfoAccessor, new CorrelationInfoEnricherOptions
            {
                OperationIdPropertyName = operationIdPropertyName,
                TransactionIdPropertyName = transactionIdPropertyName
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> class.
        /// </summary>
        /// <param name="correlationInfoAccessor">The accessor implementation for the custom <see cref="CorrelationInfo"/> model.</param>
        /// <param name="options">The user-configurable options to change the behavior of the enricher.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="correlationInfoAccessor"/> is <c>null</c>.</exception>
        public CorrelationInfoEnricher(
            ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor, 
            CorrelationInfoEnricherOptions options)
        {
            if (correlationInfoAccessor is null)
            {
                throw new ArgumentNullException(nameof(correlationInfoAccessor), "Requres a correlation accessor to enrich the log events with correlation information");
            }

            Options = options ?? new CorrelationInfoEnricherOptions();
            CorrelationInfoAccessor = correlationInfoAccessor;
        }

        /// <summary>
        /// Gets the user-configurable options to change the behavior of the enricher.
        /// </summary>
        protected CorrelationInfoEnricherOptions Options { get; }

        /// <summary>
        /// Gets the <see cref="ICorrelationInfoAccessor{TCorrelationInfo}"/> that provides the correlation model.
        /// </summary>
        protected ICorrelationInfoAccessor<TCorrelationInfo> CorrelationInfoAccessor { get; }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logEvent"/> or the <paramref name="propertyFactory"/> is <c>null</c>.</exception>
        public virtual void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent), "Requires a log event to enrich the correlation information");
            }
            if (propertyFactory is null)
            {
                throw new ArgumentNullException(nameof(propertyFactory), "Requires a log event property factory to create properties for the correlation information");
            }

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
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logEvent"/>, the <paramref name="propertyFactory"/>, or the <paramref name="correlationInfo"/> is <c>null</c>.
        /// </exception>
        protected virtual void EnrichCorrelationInfo(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TCorrelationInfo correlationInfo)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent), "Requires a log event to enrich the correlation information");
            }
            if (propertyFactory is null)
            {
                throw new ArgumentNullException(nameof(propertyFactory), "Requires a log event property factory to create properties for the correlation information");
            }
            if (correlationInfo is null)
            {
                throw new ArgumentNullException(nameof(correlationInfo), "Requires the correlation information to enrich the log event");
            }
            
            EnrichLogPropertyIfPresent(logEvent, propertyFactory, Options.OperationIdPropertyName, correlationInfo.OperationId);
            EnrichLogPropertyIfPresent(logEvent, propertyFactory, Options.TransactionIdPropertyName, correlationInfo.TransactionId);
            EnrichLogPropertyIfPresent(logEvent, propertyFactory, Options.OperationParentIdPropertyName, correlationInfo.OperationParentId);
        }

        /// <summary>
        /// Adds new log property to the <paramref name="logEvent"/> when the <paramref name="propertyValue"/> is present.
        /// </summary>
        /// <param name="logEvent">The log event to enrich with property.</param>
        /// <param name="propertyFactory">The log property factory to create log properties.</param>
        /// <param name="propertyName">The name of the log property.</param>
        /// <param name="propertyValue">The value of the log property.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logEvent"/> or the <paramref name="propertyFactory"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyName"/> is blank.</exception>
        protected void EnrichLogPropertyIfPresent(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, string propertyName, string propertyValue)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent), "Requires a log event to enrich the correlation information");
            }
            if (propertyFactory is null)
            {
                throw new ArgumentNullException(nameof(propertyFactory), "Requires a log event property factory to create properties for the correlation information");
            }
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName), "Requires a non-blank name for the correlation log property");
            }
            
            if (!String.IsNullOrEmpty(propertyValue))
            {
                LogEventProperty property = propertyFactory.CreateProperty(propertyName, propertyValue);
                logEvent.AddPropertyIfAbsent(property);
            }
        }
    }
}
