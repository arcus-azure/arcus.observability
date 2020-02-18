using GuardNet;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    ///     Represents the correlation ID information on the incoming requests and outgoing responses.
    /// </summary>
    public class CorrelationInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CorrelationInfo" /> class.
        /// </summary>
        public CorrelationInfo(string operationId, string transactionId)
        {
            Guard.NotNullOrEmpty(operationId, nameof(operationId),
                "Cannot create a correlation instance with a blank operation ID");

            OperationId = operationId;
            TransactionId = transactionId;
        }

        /// <summary>
        ///     Gets the ID that relates different requests together.
        /// </summary>
        public string TransactionId { get; }

        /// <summary>
        ///     Gets the unique ID information of the request.
        /// </summary>
        public string OperationId { get; }
    }
}