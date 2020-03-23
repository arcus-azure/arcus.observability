using Arcus.Observability.Correlation;

namespace Arcus.Observability.Tests.Unit.Correlation 
{
    /// <summary>
    /// Test model to extend the <see cref="CorrelationInfo"/>.
    /// </summary>
    public class TestCorrelationInfo : CorrelationInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CorrelationInfo" /> class.
        /// </summary>
        public TestCorrelationInfo(string operationId, string transactionId, string testId) 
            : base(operationId, transactionId)
        {
            TestId = testId;
        }

        /// <summary>
        /// Gets the test identifier for this correlation information model.
        /// </summary>
        public string TestId { get; }
    }
}