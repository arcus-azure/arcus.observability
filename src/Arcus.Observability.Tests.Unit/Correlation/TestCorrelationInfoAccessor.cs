using System;
using Arcus.Observability.Correlation;

namespace Arcus.Observability.Tests.Unit.Correlation 
{
    /// <summary>
    /// Test <see cref="ICorrelationInfoAccessor{TCorrelationInfo}"/> implementation to access the <see cref="TestCorrelationInfo"/> model
    /// </summary>
    public class TestCorrelationInfoAccessor : ICorrelationInfoAccessor<TestCorrelationInfo>
    {
        /// <summary>
        /// Gets or sets the current correlation information initialized in this context.
        /// </summary>
        public TestCorrelationInfo CorrelationInfo { get; set; }
    }
}