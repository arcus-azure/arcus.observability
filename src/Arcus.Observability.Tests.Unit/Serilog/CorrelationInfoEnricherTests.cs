using System;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Unit.Correlation;
using Moq;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class CorrelationInfoEnricherTests
    {
        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessor_HasOperationIdAndTransactionId()
        {
            // Arrange
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = DefaultCorrelationInfoAccessor.Instance;
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo()
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationId, expectedOperationId),
                $"Expected to have a log property operation ID '{ContextProperties.Correlation.OperationId}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
        }

        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorT_HasOperationIdAndTransactionId()
        {
            // Arrange
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = DefaultCorrelationInfoAccessor<TestCorrelationInfo>.Instance;
            correlationInfoAccessor.SetCorrelationInfo(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo<TestCorrelationInfo>()
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationId, expectedOperationId),
                $"Expected to have a log property operation ID '{ContextProperties.Correlation.OperationId}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
            Assert.False(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }

        [Fact]
        public void LogEvent_WithCorrelationInfo_HasOperationIdAndTransactionId()
        {
            // Arrange
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, expectedTransactionId));
            
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(stubAccessor.Object)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationId, expectedOperationId),
                $"Expected to have a log property operation ID '{ContextProperties.Correlation.OperationId}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
        }

        [Fact]
        public void LogEvent_WithCustomCorrelationInfoEnricher_HasCustomEnrichedInformation()
        {
            // Arrange
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor<TestCorrelationInfo>>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(new TestCorrelationInfoEnricher(stubAccessor.Object))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with custom correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationId, expectedOperationId),
                $"Expected to have a log property operation ID '{ContextProperties.Correlation.OperationId}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
            Assert.True(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }
    }
}
