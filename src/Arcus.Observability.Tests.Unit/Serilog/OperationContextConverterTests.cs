using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Bogus;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class OperationContextConverterTests
    {
        private static readonly Faker BogusGenerator = new Faker();
        private static readonly OperationContextConverter DefaultConverter = new OperationContextConverter(new ApplicationInsightsSinkOptions());

        public static IEnumerable<object[]> NonRequestTelemetry => new[]
        {
            new object[] { new TraceTelemetry() },
            new object[] { new ExceptionTelemetry() },
            new object[] { new DependencyTelemetry() },
            new object[] { new MetricTelemetry() },
            new object[] { new EventTelemetry() }
        };

        [Theory]
        [MemberData(nameof(NonRequestTelemetry))]
        public void EnrichWithCorrelationInfo_WithNonRequestTelemetry_Succeeds<TEntry>(TEntry telemetry) 
            where TEntry : ITelemetry, ISupportProperties
        {
            // Arrange
            var operationId = $"operation-{Guid.NewGuid()}";
            var transactionId = $"transaction-{Guid.NewGuid()}";
            telemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            telemetry.Properties.Add(ContextProperties.Correlation.TransactionId, transactionId);

            // Act
            DefaultConverter.EnrichWithCorrelationInfo(telemetry);

            // Assert
            Assert.Equal(operationId, telemetry.Context.Operation.ParentId);
            Assert.Equal(transactionId, telemetry.Context.Operation.Id);
        }

        [Theory]
        [MemberData(nameof(NonRequestTelemetry))]
        public void EnrichWithCorrelationInfo_WithNonRequestTelemetry_IgnoresCorrelation<TEntry>(TEntry telemetry)
            where TEntry : ITelemetry, ISupportProperties
        {
            // Act
            DefaultConverter.EnrichWithCorrelationInfo(telemetry);

            // Assert
            Assert.Null(telemetry.Context.Operation.ParentId);
            Assert.Null(telemetry.Context.Operation.Id);
        }

        [Fact]
        public void EnrichWithCorrelationInfo_RequestWithOperationId_OperationIdSetOnContext()
        {
            // Arrange
            var operationId = $"operation-{Guid.NewGuid()}";
            var transactionId = $"transaction-{Guid.NewGuid()}";
            var operationParentId = $"parent-{Guid.NewGuid()}";
            var telemetry = new RequestTelemetry();
            telemetry.Properties.Add(ContextProperties.Correlation.OperationId, operationId);
            telemetry.Properties.Add(ContextProperties.Correlation.TransactionId, transactionId);
            telemetry.Properties.Add(ContextProperties.Correlation.OperationParentId, operationParentId);

            // Act
            DefaultConverter.EnrichWithCorrelationInfo(telemetry);

            // Assert
            Assert.Equal(operationId, telemetry.Id);
            Assert.Equal(transactionId, telemetry.Context.Operation.Id);
            Assert.Equal(operationParentId, telemetry.Context.Operation.ParentId);
        }

        [Fact]
        public void EnrichWithOperationName_RequestWithRequestNameAndOperationName_OperationNameSetOnContext()
        {
            // Arrange
            var telemetryName = BogusGenerator.Random.Word();
            var operationName = BogusGenerator.Random.Word();
            var traceTelemetry = new RequestTelemetry();
            traceTelemetry.Name = telemetryName;
            traceTelemetry.Context.Operation.Name = operationName;

            // Act
            DefaultConverter.EnrichWithOperationName(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(operationName, traceTelemetry.Context.Operation.Name);
        }

        [Fact]
        public void EnrichWithOperationName_RequestWithRequestNameWithoutOperationName_NoOperationNameSetOnContext()
        {
            // Arrange
            var telemetryName = BogusGenerator.Random.Word();
            var traceTelemetry = new RequestTelemetry();
            traceTelemetry.Name = telemetryName;

            // Act
            DefaultConverter.EnrichWithOperationName(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(telemetryName, traceTelemetry.Context.Operation.Name);
        }

        [Fact]
        public void EnrichWithOperationName_DependencyWithRequestName_NoOperationNameSetOnContext()
        {
            // Arrange
            var telemetryName = BogusGenerator.Random.Word();
            var traceTelemetry = new DependencyTelemetry();
            traceTelemetry.Name = telemetryName;

            // Act
            DefaultConverter.EnrichWithOperationName(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(telemetryName, traceTelemetry.Context.Operation.Name);
        }

        [Fact]
        public void EnrichWithOperationName_EventWithRequestName_NoOperationNameSetOnContext()
        {
            // Arrange
            var telemetryName = BogusGenerator.Random.Word();
            var traceTelemetry = new EventTelemetry();
            traceTelemetry.Name = telemetryName;

            // Act
            DefaultConverter.EnrichWithOperationName(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(telemetryName, traceTelemetry.Context.Operation.Name);
        }

        [Fact]
        public void EnrichWithOperationName_AvailabilityWithRequestName_NoOperationNameSetOnContext()
        {
            // Arrange
            var telemetryName = BogusGenerator.Random.Word();
            var traceTelemetry = new AvailabilityTelemetry();
            traceTelemetry.Name = telemetryName;

            // Act
            DefaultConverter.EnrichWithOperationName(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(telemetryName, traceTelemetry.Context.Operation.Name);
        }

        [Fact]
        public void EnrichWithOperationName_MetricWithRequestName_NoOperationNameSetOnContext()
        {
            // Arrange
            var telemetryName = BogusGenerator.Random.Word();
            var traceTelemetry = new MetricTelemetry();
            traceTelemetry.Name = telemetryName;

            // Act
            DefaultConverter.EnrichWithOperationName(traceTelemetry);

            // Assert
            Assert.NotNull(traceTelemetry);
            Assert.NotNull(traceTelemetry.Context);
            Assert.NotNull(traceTelemetry.Context.Operation);
            Assert.Equal(telemetryName, traceTelemetry.Context.Operation.Name);
        }
    }
}
