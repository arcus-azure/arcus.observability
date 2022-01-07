using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Sinks.ApplicationInsights
{
    public class ApplicationInsightsSinkRequestOptionsTests
    {
        [Fact]
        public void DefaultGenerateId_WithNoCustomFunction_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkRequestOptions();
            
            // Act
            string id = options.GenerateId();
            
            // Assert
            Assert.False(string.IsNullOrWhiteSpace(id));
        }
        
        [Fact]
        public void SetGenerateId_WithCustomFunction_Succeeds()
        {
            // Arrange
            var options = new ApplicationInsightsSinkRequestOptions();
            var id = Guid.NewGuid().ToString();
            
            // Act
            options.GenerateId = () => id;
            
            // Assert
            Assert.Equal(id, options.GenerateId());
        }

        [Fact]
        public void SetGenerateId_WithoutFunction_Fails()
        {
            // Arrange
            var options = new ApplicationInsightsSinkRequestOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => options.GenerateId = null);
        }
    }
}
