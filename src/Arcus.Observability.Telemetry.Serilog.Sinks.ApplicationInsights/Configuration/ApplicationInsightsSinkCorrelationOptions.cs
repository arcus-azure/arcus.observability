using System;
using Arcus.Observability.Telemetry.Core;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration
{
    /// <summary>
    /// User-defined configuration options to influence the behavior of the Azure Application Insights Serilog sink while assigning telemetry correlation.
    /// </summary>
    public class ApplicationInsightsSinkCorrelationOptions
    {
        private string _operationIdPropertyName = ContextProperties.Correlation.OperationId,
                       _transactionIdPropertyName = ContextProperties.Correlation.TransactionId,
                       _operationParentIdPropertyName = ContextProperties.Correlation.OperationParentId;

        /// <summary>
        /// Gets or sets the Serilog application property name where the operation ID is set.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string OperationIdPropertyName
        {
            get => _operationIdPropertyName;
            set
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                _operationIdPropertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the Serilog application property name where the transaction ID is set.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string TransactionIdPropertyName
        {
            get => _transactionIdPropertyName;
            set
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                _transactionIdPropertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the Serilog application property name where the correlation operation parent ID is set.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string OperationParentIdPropertyName
        {
            get => _operationParentIdPropertyName;
            set
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);
                _operationParentIdPropertyName = value;
            }
        }
    }
}
