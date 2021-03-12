using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Sinks.ApplicationInsights
{
    public class LoggerConfigurationExtensionsTests
    {
        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsights_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsights(instrumentationKey));
        }
        
        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithConfigOptions_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsights(instrumentationKey, options => { }));
        }
        
        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithMinLogLevel_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsights(instrumentationKey, LogEventLevel.Debug));
        }
        
        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithMinLogLevelWithConfigOptions_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsights(instrumentationKey, LogEventLevel.Debug, options => { }));
        }
    }
}
