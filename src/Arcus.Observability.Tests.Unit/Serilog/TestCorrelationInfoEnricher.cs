using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Unit.Correlation;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Tests.Unit.Serilog 
{
    /// <summary>
    /// Test implementation for the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> using the test <see cref="TestCorrelationInfo"/> model.
    /// </summary>
    public class TestCorrelationInfoEnricher : CorrelationInfoEnricher<TestCorrelationInfo>
    {
        public const string TestId = "TestId";

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoEnricher{TCorrelationInfo}"/> class.
        /// </summary>
        /// <param name="correlationInfoAccessor">The accessor implementation for the custom <see cref="CorrelationInfo"/> model.</param>
        public TestCorrelationInfoEnricher(ICorrelationInfoAccessor<TestCorrelationInfo> correlationInfoAccessor) :
            base(correlationInfoAccessor)
        {
        }

        /// <summary>
        /// Enrich additional information from the <see cref="TestCorrelationInfo"/> type.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        /// <param name="correlationInfo">The custom correlation information model.</param>
        /// <remarks>
        ///     The <see cref="CorrelationInfo.OperationId"/> and <see cref="CorrelationInfo.TransactionId"/> are already enriched as log properties.
        /// </remarks>
        protected override void EnrichAdditionalCorrelationInfo(
            LogEvent logEvent,
            ILogEventPropertyFactory propertyFactory,
            TestCorrelationInfo correlationInfo)
        {
            LogEventProperty property = propertyFactory.CreateProperty(TestId, correlationInfo.TestId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}