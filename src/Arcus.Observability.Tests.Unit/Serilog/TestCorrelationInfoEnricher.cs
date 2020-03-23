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
        /// Enrich the <paramref name="logEvent"/> with the given <paramref name="correlationInfo"/> model.
        /// </summary>
        /// <param name="logEvent">The log event to enrich with correlation information.</param>
        /// <param name="propertyFactory">The log property factory to create log properties with correlation information.</param>
        /// <param name="correlationInfo">The correlation model that contains the current correlation information.</param>
        protected override void EnrichCorrelationInfo(
            LogEvent logEvent,
            ILogEventPropertyFactory propertyFactory,
            TestCorrelationInfo correlationInfo)
        {
            LogEventProperty property = propertyFactory.CreateProperty(TestId, correlationInfo.TestId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}