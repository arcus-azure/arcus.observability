using System;
using Arcus.Observability.Correlation;

namespace Arcus.Observability.Tests.Unit.Correlation 
{
    /// <summary>
    /// Test <see cref="ICorrelationInfoAccessor{TCorrelationInfo}"/> implementation to access the <see cref="TestCorrelationInfo"/> model
    /// </summary>
    public class TestCorrelationInfoAccessor : ICorrelationInfoAccessor<TestCorrelationInfo>
    {
        private TestCorrelationInfo _correlationInfo;

        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        public TestCorrelationInfo GetCorrelationInfo()
        {
            return _correlationInfo;
        }

        /// <summary>
        /// Sets the current correlation information for this context.
        /// </summary>
        /// <param name="correlationInfo">The correlation model to set.</param>
        public void SetCorrelationInfo(TestCorrelationInfo correlationInfo)
        {
            _correlationInfo = correlationInfo;
        }
    }
}