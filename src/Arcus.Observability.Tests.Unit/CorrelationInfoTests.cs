using System;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Xunit;

namespace Arcus.Observability.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class CorrelationInfoTests
    {
        [Fact]
        public void Constructor_Valid_Succeeds()
        {
            // Arrange
            var transactionId = Guid.NewGuid().ToString();
            var operationId = Guid.NewGuid().ToString();

            // Act
            var messageCorrelationInfo = new CorrelationInfo(operationId, transactionId);

            // Assert
            Assert.Equal(operationId, messageCorrelationInfo.OperationId);
            Assert.Equal(transactionId, messageCorrelationInfo.TransactionId);
        }

        [Fact]
        public void Constructor_NoTransactionIdSpecified_Succeeds()
        {
            // Arrange
            var operationId = Guid.NewGuid().ToString();

            // Act
            var messageCorrelationInfo = new CorrelationInfo(operationId, transactionId: null);

            // Assert
            Assert.Equal(operationId, messageCorrelationInfo.OperationId);
            Assert.Null(messageCorrelationInfo.TransactionId);
        }

        [Fact]
        public void Constructor_NoOperationIdSpecified_ThrowsException()
        {
            // Arrange
            var transactionId = Guid.NewGuid().ToString();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CorrelationInfo(operationId: null, transactionId: transactionId));
        }
    }
}
