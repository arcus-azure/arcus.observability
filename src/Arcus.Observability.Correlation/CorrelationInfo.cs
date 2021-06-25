using System;
using GuardNet;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Represents the correlation ID information on the incoming requests and outgoing responses.
    /// </summary>
    public class CorrelationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfo" /> class.
        /// </summary>
        /// <param name="operationId">The unique ID information to identify the request.</param>
        /// <param name="transactionId">The unique ID information that related different requests together in a single transaction.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationId"/> is blank.</exception>
        public CorrelationInfo(string operationId, string transactionId)
            : this(operationId, transactionId, operationParentId: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfo" /> class.
        /// </summary>
        /// <param name="operationId">The unique ID information to identify the request.</param>
        /// <param name="transactionId">The ID information that related different requests together in a single transaction.</param>
        /// <param name="operationParentId">The ID information of the original service that initiated this request.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationId"/> is blank.</exception>
        public CorrelationInfo(string operationId, string transactionId, string operationParentId)
        {
            Guard.NotNullOrEmpty(operationId, nameof(operationId), "Requires a non-blank operation ID to create a correlation instance");

            OperationId = operationId;
            TransactionId = transactionId;
            OperationParentId = operationParentId;
        }

        /// <summary>
        /// Gets the ID that relates different requests together in a single transaction.
        /// </summary>
        public string TransactionId { get; }

        /// <summary>
        /// Gets the unique ID information of the request.
        /// </summary>
        public string OperationId { get; }
        
        /// <summary>
        /// Gets the ID of the original service that initiated this request.
        /// </summary>
        public string OperationParentId { get; }
    }
}