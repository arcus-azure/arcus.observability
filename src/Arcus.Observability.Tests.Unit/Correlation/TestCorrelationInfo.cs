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
        public TestCorrelationInfo(string operationId, string transactionId) : base(operationId, transactionId) { }
    }
}