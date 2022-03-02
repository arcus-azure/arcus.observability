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
            await SetCorrelationInfo(firstOperationId, transactionId);

            // Act
            await SetCorrelationInfo(secondOperationId, transactionId);

            // Assert
            CorrelationInfo correlationInfo = DefaultCorrelationInfoAccessor.Instance.GetCorrelationInfo();
            Assert.Equal(secondOperationId, correlationInfo.OperationId);
            Assert.Equal(transactionId, correlationInfo.TransactionId);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        // Disabled the warning since declaring this method as async was on-purpose
        private async Task SetCorrelationInfo(string operationId, string transactionId)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            DefaultCorrelationInfoAccessor.Instance.SetCorrelationInfo(
                new CorrelationInfo(operationId, transactionId));
        }
    }
}
