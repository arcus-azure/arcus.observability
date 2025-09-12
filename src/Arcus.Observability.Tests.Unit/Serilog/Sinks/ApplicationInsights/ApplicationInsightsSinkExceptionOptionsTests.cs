using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Sinks.ApplicationInsights
{
    public class ApplicationInsightsSinkExceptionOptionsTests
    {
        [Theory]
        [InlineData("Exception-{0}-{1}")]
        [InlineData("Exception.{0}.Inner.{0}")]
        [InlineData("Exception-")]
        public void SetPropertyFormat_WithInvalidStringFormat_Fails(string propertyFormat)
        {
            // Arrange
            var options = new ApplicationInsightsSinkExceptionOptions();

            // Act / Assert
            Assert.Throws<FormatException>(() => options.PropertyFormat = propertyFormat);
        }

        [Theory]
        [InlineData("Exception.{0}")]
        [InlineData("{0} of exception")]
        [InlineData("Property '{0}' of exception")]
        public void SetPropertyFormat_WithCorrectStringFormat_Succeeds(string propertyFormat)
        {
            // Arrange
            var options = new ApplicationInsightsSinkExceptionOptions();

            // Act / Assert
            options.PropertyFormat = propertyFormat;
        }
    }
}
