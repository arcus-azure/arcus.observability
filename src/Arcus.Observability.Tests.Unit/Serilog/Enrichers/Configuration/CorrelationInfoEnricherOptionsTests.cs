using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers.Configuration;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Enrichers.Configuration
{
    [Trait("Category", "Unit")]
    public class CorrelationInfoEnricherOptionsTests
    {
        [Fact]
        public void SetOperationIdPropertyName_WithValue_GetsValue()
        {
            // Arrange
            var options = new CorrelationInfoEnricherOptions();
            var expected = $"operation-{Guid.NewGuid()}";
            
            // Act
            options.OperationIdPropertyName = expected;
            
            // Assert
            Assert.Equal(expected, options.OperationIdPropertyName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void SetOperationIdPropertyName_WithBlankValue_Fails(string operationIdPropertyName)
        {
            // Arrange
            var options = new CorrelationInfoEnricherOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.OperationIdPropertyName = operationIdPropertyName);
        }

        [Fact]
        public void SetTransactionIdPropertyName_WithValue_GetsValue()
        {
            // Arrange
            var options = new CorrelationInfoEnricherOptions();
            var expected = $"transaction-{Guid.NewGuid()}";
            
            // Act
            options.TransactionIdPropertyName = expected;
            
            // Assert
            Assert.Equal(expected, options.TransactionIdPropertyName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void SetTransactionIdPropertyName_WithBlankValue_Fails(string transactionIdPropertyName)
        {
            // Arrange
            var options = new CorrelationInfoEnricherOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.TransactionIdPropertyName = transactionIdPropertyName);
        }

        [Fact]
        public void SetOperationParentIdPropertyName_WithValue_GetsValue()
        {
            // Arrange
            var options = new CorrelationInfoEnricherOptions();
            var expected = $"operation-parent-{Guid.NewGuid()}";
            
            // Act
            options.OperationParentIdPropertyName = expected;
            
            // Assert
            Assert.Equal(expected, options.OperationParentIdPropertyName);
        }
        
        [Theory]
        [ClassData(typeof(Blanks))]
        public void SetOperationParentIdPropertyName_WithBlankValue_Fails(string operationParentIdPropertyName)
        {
            // Arrange
            var options = new CorrelationInfoEnricherOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                options.OperationParentIdPropertyName = operationParentIdPropertyName);
        }
    }
}
