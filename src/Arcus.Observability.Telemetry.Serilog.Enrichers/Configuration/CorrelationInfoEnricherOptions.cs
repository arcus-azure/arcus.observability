using System;
using Arcus.Observability.Telemetry.Core;
using GuardNet;

namespace Arcus.Observability.Telemetry.Serilog.Enrichers.Configuration
{
    /// <summary>
    /// Represents the consumer-configurable options model to change the behavior of the Serilog <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/>.
    /// </summary>
    public class CorrelationInfoEnricherOptions
    {
        private string _operationIdPropertyName = ContextProperties.Correlation.OperationId,
                       _transactionIdPropertyName = ContextProperties.Correlation.TransactionId,
                       _operationParentIdPropertyName = ContextProperties.Correlation.OperationParentId;

        /// <summary>
        /// Gets or sets the property name to enrich the log event with the correlation information operation ID.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string OperationIdPropertyName
        {
            get => _operationIdPropertyName;
            set
            {
                Guard.NotNullOrWhitespace(value, nameof(value), "Requires a non-blank property name to enrich the log event with the correlation information operation ID");
                _operationIdPropertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the property name to enrich the log event with the correlation information transaction ID.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string TransactionIdPropertyName
        {
            get => _transactionIdPropertyName;
            set
            {
                Guard.NotNullOrWhitespace(value, nameof(value), "Requires a non-blank property name to enrich the log event with the correlation information transaction ID");
                _transactionIdPropertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the property name to enrich the log event with the correlation information parent operation ID.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string OperationParentIdPropertyName
        {
            get => _operationParentIdPropertyName;
            set
            {
                Guard.NotNullOrWhitespace(value, nameof(value), "Requires a non-blank property name to enrich the log event with the correlation information parent operation ID");
                _operationParentIdPropertyName = value;
            }
        }
    }
}
