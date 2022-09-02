using System;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Options for handling correlation id on incoming requests.
    /// </summary>
    [Obsolete("Use HTTP, messaging, or custom-specific correlation options instead")]
    public class CorrelationInfoOptions
    {
        /// <summary>
        /// Gets the correlation options specific for the transaction ID.
        /// </summary>
        public CorrelationInfoTransactionOptions Transaction { get; } = new CorrelationInfoTransactionOptions();

        /// <summary>
        /// Gets the correlation options specific for the operation ID.
        /// </summary>
        public CorrelationInfoOperationOptions Operation { get; } = new CorrelationInfoOperationOptions();

        /// <summary>
        /// Gets the correlation options specific for the upstream service.
        /// </summary>
        public CorrelationInfoUpstreamServiceOptions OperationParent { get; } = new CorrelationInfoUpstreamServiceOptions();
    }
}
