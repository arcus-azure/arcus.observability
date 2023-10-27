using System;
using Azure.Identity;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Sinks.ApplicationInsights
{
    public class LoggerConfigurationExtensionsTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsUsingManagedIdentity_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingManagedIdentity(provider, connectionString, "<client-id>"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsUsingManagedIdentityWithLevel_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingManagedIdentity(provider, connectionString, "<client-id>", level));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsUsingManagedIdentityWithOptions_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingManagedIdentity(provider, connectionString, "<client-id>", level, opt => { }));
        }

        [Fact]
        public void AzureApplicationInsightsUsingManagedIdentity_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsUsingManagedIdentity(provider, "<connection-string>", "<client-id>"));
        }

        [Fact]
        public void AzureApplicationInsightsUsingAuthentication_WithoutTokenCredential_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, "<connection-string>", tokenCredential: null));
        }

        [Fact]
        public void AzureApplicationInsightsUsingAuthenticationWithLogLevel_WithoutTokenCredential_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, "<connection-string>", tokenCredential: null, level));
        }
        
        [Fact]
        public void AzureApplicationInsightsUsingAuthenticationWithOptions_WithoutTokenCredential_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, "<connection-string>", tokenCredential: null, level, opt => { }));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsUsingAuthentication_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var credential = new DefaultAzureCredential();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, connectionString, credential));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsUsingAuthenticationWithLevel_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var credential = new DefaultAzureCredential();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, connectionString, credential, level));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AzureApplicationInsightsUsingAuthenticationWithOptions_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var credential = new DefaultAzureCredential();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, connectionString, credential, level, opt => { }));
        }

        [Fact]
        public void AzureApplicationInsightsUsingAuthentication_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var credential = new DefaultAzureCredential();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsUsingAuthentication(provider, "<connection-string>", credential));
        }

        [Fact]
        public void AzureApplicationInsightsWithInstrumentationKey_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, "<key>"));
        }

        [Fact]
        public void AzureApplicationInsightsWithInstrumentationKeyWithLogLevel_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, "<key>", level));
        }

        [Fact]
        public void AzureApplicationInsightsWithInstrumentationKeyWithOptions_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, "<key>", options => { }));
        }

        [Fact]
        public void AzureApplicationInsightsWithInstrumentationKeyWithLevelAndOptions_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, "<key>", level, options => { }));
        }

         [Fact]
        public void AzureApplicationInsightsWithConnectionString_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(provider, "<connection-string>"));
        }

        [Fact]
        public void AzureApplicationInsightsWithConnectionStringWithLogLevel_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(provider, "<connection-string>", level));
        }

        [Fact]
        public void AzureApplicationInsightsWithConnectionStringWithOptions_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(provider, "<connection-striong>", options => { }));
        }

        [Fact]
        public void AzureApplicationInsightsWithConnectionStringWithLevelAndOptions_WithoutTelemetryClient_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();
            var config = new LoggerConfiguration();
            var level = BogusGenerator.PickRandom<LogEventLevel>();

            // Act / Assert
            Assert.ThrowsAny<InvalidOperationException>(
                () => config.WriteTo.AzureApplicationInsightsWithConnectionString(provider, "<connection-string>", level, options => { }));
        }

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
