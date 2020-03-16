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
    public class CorrelationInfoEnricher : ILogEventEnricher
    {
        private readonly ICorrelationInfoAccessor _correlationInfoAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoEnricher"/> class.
        /// </summary>
        /// <param name="correlationInfoAccessor">The accessor implementation for the <see cref="CorrelationInfo"/> model.</param>
        public CorrelationInfoEnricher(ICorrelationInfoAccessor correlationInfoAccessor)
        {
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor));

            _correlationInfoAccessor = correlationInfoAccessor;
        }

        /// <summary>Enrich the log event.</summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            Guard.NotNull(logEvent, nameof(logEvent));
            Guard.NotNull(propertyFactory, nameof(propertyFactory));

            CorrelationInfo correlationInfo = _correlationInfoAccessor.CorrelationInfo;

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
        }
    }
}
