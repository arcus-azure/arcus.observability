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
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Requires a non-blank Serilog application property name to locate the correlation operation ID", nameof(value));
                }

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
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Requires a non-blank Serilog application property name to locate the correlation transaction ID", nameof(value));
                }

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
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Requires a non-blank Serilog application property name to locate the correlation operation parent ID", nameof(value));
                }

                _operationParentIdPropertyName = value;
            }
        }
    }
}
