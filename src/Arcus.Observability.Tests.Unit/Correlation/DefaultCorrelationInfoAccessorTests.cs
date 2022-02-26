using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Correlation
{
    public class DefaultCorrelationInfoAccessorTests
    {
        [Fact]
        public void SetCorrelationInfo_Twice_UsesMostRecentValue()
        {
            // Arrange
            var firstOperationId = $"operation-{Guid.NewGuid()}";
            var secondOperationId = $"operation-{Guid.NewGuid()}";
            var transactionId = $"transaction-{Guid.NewGuid()}";
            SetCorrelationInfo(firstOperationId, transactionId);

            // Act
            SetCorrelationInfo(secondOperationId, transactionId);

            // Assert
            CorrelationInfo correlationInfo = DefaultCorrelationInfoAccessor.Instance.GetCorrelationInfo();
            Assert.Equal(secondOperationId, correlationInfo.OperationId);
            Assert.Equal(transactionId, correlationInfo.TransactionId);
        }

        private void SetCorrelationInfo(string operationId, string transactionId)
        {
            DefaultCorrelationInfoAccessor.Instance.SetCorrelationInfo(
                new CorrelationInfo(operationId, transactionId));
        }
    }
}
