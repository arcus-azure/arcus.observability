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
        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> class.
        /// </summary>
        /// <param name="correlationInfoAccessor">The accessor implementation for the custom <see cref="CorrelationInfo"/> model.</param>
        public CorrelationInfoEnricher(ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor)
        {
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor));

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

            if (!String.IsNullOrEmpty(correlationInfo.OperationId))
            {
                LogEventProperty property = propertyFactory.CreateProperty(ContextProperties.Correlation.OperationId, correlationInfo.OperationId);
                logEvent.AddPropertyIfAbsent(property);
            }

            if (!String.IsNullOrEmpty(correlationInfo.TransactionId))
            {
                LogEventProperty property = propertyFactory.CreateProperty(ContextProperties.Correlation.TransactionId, correlationInfo.TransactionId);
                logEvent.AddPropertyIfAbsent(property);
            }

            EnrichAdditionalCorrelationInfo(logEvent, propertyFactory, correlationInfo);
        }

        /// <summary>
        /// Enrich additional information from the correlation model.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        /// <param name="correlationInfo">The custom correlation information model.</param>
        /// <remarks>
        ///     The <see cref="CorrelationInfo.OperationId"/> and <see cref="CorrelationInfo.TransactionId"/> are already enriched as log properties.
        /// </remarks>
        protected virtual void EnrichAdditionalCorrelationInfo(
            LogEvent logEvent,
            ILogEventPropertyFactory propertyFactory,
            TCorrelationInfo correlationInfo)
        {
        }
    }
}
