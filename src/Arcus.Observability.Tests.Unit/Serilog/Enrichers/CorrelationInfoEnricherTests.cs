using System;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Arcus.Observability.Tests.Unit.Correlation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Enrichers
{
    [Trait("Category", "Unit")]
    public class CorrelationInfoEnricherTests
    {
        [Fact]
        public void LogEvent_WithStaticDefaultCorrelationInfoAccessor_HasOperationIdAndTransactionId()
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
        public void LogEvent_WithDefaultCorrelationInfoAccessor_HasOperationIdAndTransactionId()
        {
            // Arrange
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor)
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
        public void LogEvent_WithDefaultCorrelationInfoAccessor_HasOperationIdAndTransactionIdAndOperationParentId()
        {
            // Arrange
            var expectedOperationId = $"operation-{Guid.NewGuid()}";
            var expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            var expectedOperationParentId = $"operation-parent-{Guid.NewGuid()}";
            
            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId, expectedOperationParentId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor)
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
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationParentId, expectedOperationParentId),
                $"Expected to have a log property operation parent ID '{ContextProperties.Correlation.OperationParentId}' with the value '{expectedOperationParentId}'");
        }

        [Fact]
        public void LogEvent_WithStaticDefaultCorrelationInfoAccessorWithCustomOperationIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = DefaultCorrelationInfoAccessor.Instance;
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(operationIdPropertyName: operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
        }

        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorWithCustomOperationIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, operationIdPropertyName: operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
        }
        
        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorWithCustomOperationIdPropertyOptions_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, options => options.OperationIdPropertyName = operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
        }

        [Fact]
        public void LogEvent_WithStaticDefaultCorrelationInfoAccessorWithCustomTransactionIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = DefaultCorrelationInfoAccessor.Instance;
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(transactionIdPropertyName: transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
        }

        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorWithCustomTransactionIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, transactionIdPropertyName: transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
        }
        
        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorWithCustomTransactionIdPropertyOptions_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, options => options.TransactionIdPropertyName = transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
        }
        
        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorWithCustomOperationParentIdPropertyOptions_HasOperationIdAndTransactionId()
        {
            // Arrange
            var operationParentIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            var expectedOperationId = $"operation-{Guid.NewGuid()}";
            var expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            var expectedOperationParentId = $"operation-parent-{Guid.NewGuid()}";
            
            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(expectedOperationId, expectedTransactionId, expectedOperationParentId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, options => options.OperationParentIdPropertyName = operationParentIdPropertyName)
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
            Assert.True(
                logEvent.ContainsProperty(operationParentIdPropertyName, expectedOperationParentId),
                $"Expected to have a log property operation parent ID '{operationParentIdPropertyName}' with the value '{expectedOperationParentId}'");
        }

        [Fact]
        public void LogEvent_WithStaticDefaultCorrelationInfoAccessorT_HasOperationIdAndTransactionId()
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
        public void LogEvent_WithDefaultCorrelationInfoAccessorT_HasOperationIdAndTransactionId()
        {
            // Arrange
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor<TestCorrelationInfo>();
            correlationInfoAccessor.SetCorrelationInfo(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor)
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
        public void LogEvent_WithStaticDefaultCorrelationInfoAccessorTWithCustomOperationIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = DefaultCorrelationInfoAccessor<TestCorrelationInfo>.Instance;
            correlationInfoAccessor.SetCorrelationInfo(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo<TestCorrelationInfo>(operationIdPropertyName: operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
            Assert.False(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }

        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorTWithCustomOperationIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor<TestCorrelationInfo>();
            correlationInfoAccessor.SetCorrelationInfo(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, operationIdPropertyName: operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
            Assert.False(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }

        [Fact]
        public void LogEvent_WithStaticDefaultCorrelationInfoAccessorTWithCustomTransactionIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = DefaultCorrelationInfoAccessor<TestCorrelationInfo>.Instance;
            correlationInfoAccessor.SetCorrelationInfo(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo<TestCorrelationInfo>(transactionIdPropertyName: transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
            Assert.False(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }

        [Fact]
        public void LogEvent_WithDefaultCorrelationInfoAccessorTWithCustomTransactionIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            string expectedTestId = $"test-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor<TestCorrelationInfo>();
            correlationInfoAccessor.SetCorrelationInfo(new TestCorrelationInfo(expectedOperationId, expectedTransactionId, expectedTestId));

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(correlationInfoAccessor, transactionIdPropertyName: transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
            Assert.False(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }

        [Fact]
        public void LogEvent_WithCorrelationInfo_HasOperationId()
        {
            // Arrange
            var expectedOperationId = $"operation-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, transactionId: null));
            
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
            Assert.False(logEvent.Properties.ContainsKey(ContextProperties.Correlation.TransactionId));
            Assert.False(logEvent.Properties.ContainsKey(ContextProperties.Correlation.OperationParentId));
        }
        
        [Fact]
        public void LogEvent_WithCorrelationInfoWithTransactionId_HasOperationIdAndTransactionId()
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
        public void LogEvent_WithCorrelationInfoWithOperationParentId_HasOperationIdAndTransactionIdAndOperationParentId()
        {
            // Arrange
            var expectedOperationId = $"operation-{Guid.NewGuid()}";
            var expectedTransactionId = $"transaction-{Guid.NewGuid()}";
            var expectedOperationParentId = $"operation-parent-{Guid.NewGuid()}";
            
            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, expectedTransactionId, expectedOperationParentId));
            
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
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationParentId, expectedOperationParentId),
                $"Expected to have a log property operation parent ID '{ContextProperties.Correlation.OperationParentId}' with the value '{expectedOperationParentId}'");
        }

        [Fact]
        public void LogEvent_WithCorrelationInfoWithCustomOperationIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, expectedTransactionId));
            
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(stubAccessor.Object, operationIdPropertyName: operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
        }
        
        [Fact]
        public void LogEvent_WithCorrelationInfoWithCustomOperationIdPropertyOptions_HasOperationIdAndTransactionId()
        {
            // Arrange
            string operationIdPropertyName = $"operation-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, expectedTransactionId));
            
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(stubAccessor.Object, options => options.OperationIdPropertyName = operationIdPropertyName)
                .WriteTo.Sink(spySink)
                .CreateLogger();

            // Act
            logger.Information("This message will be enriched with correlation information");

            // Assert
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(operationIdPropertyName, expectedOperationId),
                $"Expected to have a log property operation ID '{operationIdPropertyName}' with the value '{expectedOperationId}'");
            Assert.True(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
            Assert.DoesNotContain(logEvent.Properties, property => property.Key == ContextProperties.Correlation.OperationParentId);
        }

        [Fact]
        public void LogEvent_WithCorrelationInfoWithCustomTransactionIdProperty_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, expectedTransactionId));
            
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(stubAccessor.Object, transactionIdPropertyName: transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
            Assert.DoesNotContain(logEvent.Properties, property => property.Key == ContextProperties.Correlation.OperationParentId);
        }
        
        [Fact]
        public void LogEvent_WithCorrelationInfoWithCustomTransactionIdPropertyOptions_HasOperationIdAndTransactionId()
        {
            // Arrange
            string transactionIdPropertyName = $"transaction-name-{Guid.NewGuid():N}";
            string expectedOperationId = $"operation-{Guid.NewGuid()}";
            string expectedTransactionId = $"transaction-{Guid.NewGuid()}";

            var spySink = new InMemoryLogSink();
            var stubAccessor = new Mock<ICorrelationInfoAccessor>();
            stubAccessor.Setup(accessor => accessor.GetCorrelationInfo())
                        .Returns(new CorrelationInfo(expectedOperationId, expectedTransactionId));
            
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithCorrelationInfo(stubAccessor.Object, options => options.TransactionIdPropertyName = transactionIdPropertyName)
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
                logEvent.ContainsProperty(transactionIdPropertyName, expectedTransactionId),
                $"Expected to have a log property transaction ID '{transactionIdPropertyName}' with the value '{expectedTransactionId}'");
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
            Assert.False(
                logEvent.ContainsProperty(ContextProperties.Correlation.OperationId, expectedOperationId),
                $"Expected not to have a log property operation ID '{ContextProperties.Correlation.OperationId}' with the value '{expectedOperationId}'");
            Assert.False(
                logEvent.ContainsProperty(ContextProperties.Correlation.TransactionId, expectedTransactionId),
                $"Expected not to have a log property transaction ID '{ContextProperties.Correlation.TransactionId}' with the value '{expectedTransactionId}'");
            Assert.True(
                logEvent.ContainsProperty(TestCorrelationInfoEnricher.TestId, expectedTestId),
                $"Expected to have a log property test ID '{TestCorrelationInfoEnricher.TestId}' with the value '{expectedTestId}'");
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankOperationIdPropertyName_Throws(string operationIdPropertyName)
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new CorrelationInfoEnricher<TestCorrelationInfo>(
                    new TestCorrelationInfoAccessor(),
                    operationIdPropertyName: operationIdPropertyName,
                    transactionIdPropertyName: "some ignored valid value"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankTransactionIdPropertyName_Throws(string transactionIdPropertyName)
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new CorrelationInfoEnricher<TestCorrelationInfo>(
                    new TestCorrelationInfoAccessor(),
                    operationIdPropertyName: "some ignored valid value",
                    transactionIdPropertyName: transactionIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfo_WithBlankOperationIdName_Throws(string operationIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(operationIdPropertyName: operationIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfo_WithBlankTransactionIdName_Throws(string transactionIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(transactionIdPropertyName: transactionIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessor_WithBlankOperationIdName_Throws(string operationIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(
                    new TestCorrelationInfoAccessor(), 
                    operationIdPropertyName: operationIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessor_WithBlankTransactionIdName_Throws(string transactionIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(
                    new TestCorrelationInfoAccessor(), 
                    transactionIdPropertyName: transactionIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessor_WithServiceProviderWithBlankOperationIdName_Throws(string operationIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            var services = new ServiceCollection();
            services.AddCorrelation();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(
                    serviceProvider, 
                    operationIdPropertyName: operationIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessor_WithServiceProviderWithBlankTransactionIdName_Throws(string transactionIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            var services = new ServiceCollection();
            services.AddCorrelation();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(
                    serviceProvider, 
                    transactionIdPropertyName: transactionIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessorT_WithBlankOperationIdName_Throws(string operationIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(
                    DefaultCorrelationInfoAccessor<TestCorrelationInfo>.Instance, 
                    operationIdPropertyName: operationIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessorT_WithBlankTransactionIdName_Throws(string transactionIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo(
                    DefaultCorrelationInfoAccessor<TestCorrelationInfo>.Instance,
                    transactionIdPropertyName: transactionIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessorT_WithServiceProviderWithBlankOperationIdName_Throws(string operationIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            var services = new ServiceCollection();
            services.AddCorrelation<TestCorrelationInfo>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo<TestCorrelationInfo>(
                    serviceProvider, 
                    operationIdPropertyName: operationIdPropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithCorrelationInfoAccessorT_WithServiceProviderWithBlankTransactionIdName_Throws(string transactionIdPropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            var services = new ServiceCollection();
            services.AddCorrelation<TestCorrelationInfo>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithCorrelationInfo<TestCorrelationInfo>(
                    serviceProvider,
                    transactionIdPropertyName: transactionIdPropertyName));
        }

        [Fact]
        public void WithCorrelationAccessor_WithoutServiceProvider_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => config.Enrich.WithCorrelationInfo(serviceProvider: null));
        }

        [Fact]
        public void WithCorrelationAccessorOptions_WithoutServiceProvider_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.Enrich.WithCorrelationInfo(
                    serviceProvider: null,
                    configureOptions: options => { }));
        }

        [Fact]
        public void WithCorrelationAccessorT_WithoutServiceProvider_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => config.Enrich.WithCorrelationInfo<TestCorrelationInfo>(serviceProvider: null));
        }

        [Fact]
        public void WithCorrelationAccessorTOptions_WithoutServiceProvider_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                config.Enrich.WithCorrelationInfo<TestCorrelationInfo>(
                    serviceProvider: null,
                    configureOptions: options => { }));
        }

        [Fact]
        public void WithCorrelationAccessor_WithoutRegisteredCorrelationAccessor_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(() => config.Enrich.WithCorrelationInfo(serviceProvider));
        }

        [Fact]
        public void WithCorrelationAccessorOptions_WithoutRegisteredCorrelationAccessor_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(() => config.Enrich.WithCorrelationInfo(serviceProvider, options => { }));
        }
        
        [Fact]
        public void WithCorrelationAccessorT_WithoutRegisteredCorrelationAccessor_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(() => config.Enrich.WithCorrelationInfo<TestCorrelationInfo>(serviceProvider));
        }

        [Fact]
        public void WIthCorrelationAccessorT_WithoutRegisteredCorrelationAccessor_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(() =>
                config.Enrich.WithCorrelationInfo<TestCorrelationInfo>(serviceProvider, options => { }));
        }

        [Fact]
        public void WithCorrelationAccessorT_WithWrongRegisteredCorrelationAccessor_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            var services = new ServiceCollection();
            services.AddCorrelation();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(() => config.Enrich.WithCorrelationInfo<TestCorrelationInfo>(serviceProvider));
        }

        [Fact]
        public void WithCorrelationAccessorTOptions_WithWrongRegisteredCorrelationAccessor_Throws()
        {
            // Arrange
            var config = new LoggerConfiguration();
            var services = new ServiceCollection();
            services.AddCorrelation();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(() =>
                config.Enrich.WithCorrelationInfo<TestCorrelationInfo>(serviceProvider, options => { }));
        }
    }
}
