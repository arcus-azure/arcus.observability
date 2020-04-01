using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class OperationContextConverterTests
    {
        [Fact]
        public void EnrichWithCorrelationInfo_TraceWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();
            var traceTelemetry = new TraceTelemetry();
            traceTelemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationId, traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_TraceWithoutOperationId_NoOperationIdSetOnContext()
        {
            // Arrange
            var traceTelemetry = new TraceTelemetry();
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Null(traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_EventWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();
            var traceTelemetry = new EventTelemetry();
            traceTelemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationId, traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_EventWithoutOperationId_NoOperationIdSetOnContext()
        {
            // Arrange
            var traceTelemetry = new EventTelemetry();
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Null(traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_MetricWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();
            var traceTelemetry = new MetricTelemetry();
            traceTelemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationId, traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_MetricWithoutOperationId_NoOperationIdSetOnContext()
        {
            // Arrange
            var traceTelemetry = new MetricTelemetry();
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Null(traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_DependencyWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();
            var traceTelemetry = new DependencyTelemetry();
            traceTelemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationId, traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_DependencyWithoutOperationId_NoOperationIdSetOnContext()
        {
            // Arrange
            var traceTelemetry = new DependencyTelemetry();
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Null(traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_RequestWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();
            var traceTelemetry = new RequestTelemetry();
            traceTelemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationId, traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_RequestWithoutOperationId_NoOperationIdSetOnContext()
        {
            // Arrange
            var traceTelemetry = new RequestTelemetry();
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Null(traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_ExceptionWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();
            var traceTelemetry = new ExceptionTelemetry();
            traceTelemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationId, traceTelemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_ExceptionWithoutOperationId_NoOperationIdSetOnContext()
        {
            // Arrange
            var traceTelemetry = new ExceptionTelemetry();
            var converter = new OperationContextConverter();

            // Act
            converter.EnrichWithCorrelationInfo(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Null(traceTelemetry.Context.Operation.Id);
        }
    }
}
