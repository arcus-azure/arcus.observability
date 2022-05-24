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

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithInstrumentationKey_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithInstrumentationKeyWithConfigOptions_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey, options => { }));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithInstrumentationKeyWithMinLogLevel_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey, LogEventLevel.Debug));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithInstrumentationKeyWithMinLogLevelWithConfigOptions_WithoutInstrumentationKey_Fails(string instrumentationKey)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey, LogEventLevel.Debug, options => { }));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithConnectionString_WithoutInstrumentationKey_Fails(string connectionString)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(connectionString));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithConnectionStringWithConfigOptions_WithoutInstrumentationKey_Fails(string connectionString)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(connectionString, options => { }));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithConnectionStringWithMinLogLevel_WithoutInstrumentationKey_Fails(string connectionString)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(connectionString, LogEventLevel.Debug));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsWithConnectionStringWithMinLogLevelWithConfigOptions_WithoutInstrumentationKey_Fails(string connectionString)
        {
            // Arrange
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.Throws<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(connectionString, LogEventLevel.Debug, options => { }));
        }
    }
}
