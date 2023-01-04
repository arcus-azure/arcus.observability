using System;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Correlation
{
    public class DefaultCorrelationInfoAccessorTests
    {
        [Fact]
        public async Task SetCorrelationInfo_Twice_UsesMostRecentValue()
        {
            // Arrange
            var firstOperationId = $"operation-{Guid.NewGuid()}";
            var secondOperationId = $"operation-{Guid.NewGuid()}";
            var transactionId = $"transaction-{Guid.NewGuid()}";
            await SetCorrelationInfoAsync(firstOperationId, transactionId);

            // Act
            await SetCorrelationInfoAsync(secondOperationId, transactionId);

            // Assert
            CorrelationInfo correlationInfo = DefaultCorrelationInfoAccessor.Instance.GetCorrelationInfo();
            Assert.Equal(secondOperationId, correlationInfo.OperationId);
            Assert.Equal(transactionId, correlationInfo.TransactionId);
        }

        private async Task SetCorrelationInfoAsync(string operationId, string transactionId)
        {
            await Task.Run(() => DefaultCorrelationInfoAccessor.Instance.SetCorrelationInfo(
                new CorrelationInfo(operationId, transactionId)));
        }
    }
}
