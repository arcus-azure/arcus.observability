using System;
using Arcus.Observability.Correlation;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Correlation
{
    [Trait("Category", "Unit")]
    public class CorrelationInfoOptionsTests
    {
        [Fact]
        public void OperationParentIdPropertyName_HasDefault_Succeeds()
        {
            // Arrange
            var options = new CorrelationInfoOptions();

            // Act / Assert
            Assert.NotEmpty(options.OperationParent.OperationParentIdHeaderName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void SetOperationParentIdPropertyName_WithBlankValue_Fails(string operationIdPropertyName)
        {
            // Arrange
            var options = new CorrelationInfoOptions();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                options.OperationParent.OperationParentIdHeaderName = operationIdPropertyName);
        }

        [Fact]
        public void OperationParentId_WithoutGeneration_Fails()
        {
            // Arrange
            var options = new CorrelationInfoOptions();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.OperationParent.GenerateId = null);
        }
    }
}
