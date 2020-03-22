using Arcus.Observability.Correlation;

namespace Arcus.Observability.Tests.Unit.Correlation 
{
    /// <summary>
    /// Test implementation of the <see cref="CorrelationInfoOptions"/>.
    /// </summary>
    public class TestCorrelationInfoOptions : CorrelationInfoOptions
    {
        /// <summary>
        /// Gets or sets the test option on this testing <see cref="CorrelationInfoOptions"/> variant.
        /// </summary>
        public string TestOption { get; set; }
    }
}