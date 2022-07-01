using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Bogus;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    public class ApplicationInsightsSinkCorrelationOptionsTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void OperationIdPropertyName_Default_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();

            // Act
            string propertyName = options.OperationIdPropertyName;

            // Assert
            Assert.NotNull(propertyName);
        }

        [Fact]
        public void OperationIdPropertyName_SetValue_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();
            string propertyName = BogusGenerator.Random.String();

            // Act
            options.OperationIdPropertyName = propertyName;

            // Assert
            Assert.Equal(propertyName, options.OperationIdPropertyName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void OperationIdPropertyName_SetBlank_Fails(string propertyName)
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.OperationIdPropertyName = propertyName);
        }

        [Fact]
        public void TransactionIdPropertyName_Default_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();

            // Act
            string propertyName = options.TransactionIdPropertyName;

            // Assert
            Assert.NotNull(propertyName);
        }

        [Fact]
        public void TransactionIdPropertyName_SetValue_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();
            string propertyName = BogusGenerator.Random.String();

            // Act
            options.TransactionIdPropertyName = propertyName;

            // Assert
            Assert.Equal(propertyName, options.TransactionIdPropertyName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void TransactionIdPropertyName_SetBlank_Fails(string propertyName)
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.TransactionIdPropertyName = propertyName);
        }

        [Fact]
        public void OperationParentIdPropertyName_Default_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();

            // Act
            string propertyName = options.OperationParentIdPropertyName;

            // Assert
            Assert.NotNull(propertyName);
        }

        [Fact]
        public void OperationParentIdPropertyName_SetValue_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();
            string propertyName = BogusGenerator.Random.String();

            // Act
            options.OperationParentIdPropertyName = propertyName;

            // Assert
            Assert.Equal(propertyName, options.OperationIdPropertyName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void OperationParentIdPropertyName_SetBlank_Fails(string propertyName)
        {
            // Arrange
            var options = new ApplicationInsightsSinkCorrelationOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.OperationParentIdPropertyName = propertyName);
        }
    }
}
