using System;
using System.Reflection;
using System.Reflection.Emit;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Arcus.Observability.Tests.Unit.Serilog;
using Arcus.Observability.Tests.Unit.Telemetry;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Serilog.Events;
using Xunit;

[assembly: AssemblyFileVersion(VersionEnricherTests.AssemblyVersion)]

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait(name: "Category", value: "Unit")]
    public class VersionEnricherTests
    {
        public const string AssemblyVersion = "1.23.0";

        [Fact]
        public void LogEvent_WithVersionEnricher_HasVersionProperty()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            LoggerConfiguration configuration = 
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion();

            ILogger logger = configuration.CreateLogger();

            // Act
            logger.Information("This log event should contain a 'version' property when written to the sink");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal("version", key);
            Assert.True(Version.TryParse(value.ToString().Trim('\"'), out Version result));
        }

        [Fact]
        public void LogEvent_WithCustomPropertyName_HasCustomVersionProperty()
        {
            // Arrange
            string propertyName = $"my-version-{Guid.NewGuid():N}";
            var spy = new InMemoryLogSink();
            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(propertyName)
                    .CreateLogger();

            // Act
            logger.Information("This log event should contain a custom version property when written to the sink");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(propertyName, key);
            Assert.True(Version.TryParse(value.ToString().Trim('\"'), out Version result));
        }

        [Fact]
        public void LogEvent_WithCustomAppVersion_HasCustomVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            string expected = $"version-{Guid.NewGuid()}";
            var stubAppVersion = new Mock<IAppVersion>();
            stubAppVersion.Setup(v => v.GetVersion()).Returns(expected);
            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(stubAppVersion.Object)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched with a custom application version");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(VersionEnricher.DefaultPropertyName, key);
            Assert.Equal(expected, value.ToDecentString());
        }

        [Fact]
        public void LogEvent_WithCustomAppVersionAndCustomPropertyName_HasCustomVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            string propertyName = $"version-name-{Guid.NewGuid()}";
            string expected = $"version-{Guid.NewGuid()}";
            var stubAppVersion = new Mock<IAppVersion>();
            stubAppVersion.Setup(v => v.GetVersion()).Returns(expected);
            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(stubAppVersion.Object, propertyName)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched with a custom application version on a custom property");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(propertyName, key);
            Assert.Equal(expected, value.ToDecentString());
        }

        [Fact]
        public void LogEvent_WithCustomRegisteredAppVersion_UsesCustomAppVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var expected = $"version-{Guid.NewGuid()}";
            var services = new ServiceCollection();
            services.AddAppVersion(provider => new StubAppVersion(expected));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(serviceProvider)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched by the registered stubbed app version");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(VersionEnricher.DefaultPropertyName, key);
            Assert.Equal(expected, value.ToDecentString());
        }

        [Fact]
        public void LogEvent_WithCustomAppVersionType_UsesCustomAppVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var appVersion = new AssemblyAppVersion();
            string expected = appVersion.GetVersion();
            var services = new ServiceCollection();
            services.AddAppVersion<AssemblyAppVersion>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(serviceProvider)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched by the registered app version");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(VersionEnricher.DefaultPropertyName, key);
            Assert.Equal(expected, value.ToDecentString());
        }

        [Fact]
        public void LogEvent_WithCustomGenericConsumerType_UsesConsumerTypeForAssemblyAppVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var services = new ServiceCollection();
            services.AddAssemblyAppVersion<VersionEnricherTests>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(serviceProvider)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched with a assembly version from a custom consumer type");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(VersionEnricher.DefaultPropertyName, key);
            Assert.Equal(AssemblyVersion, value.ToDecentString());
        }

        [Fact]
        public void LogEvent_WithCustomConsumerType_UsesConsumerTypeForAssemblyAppVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var services = new ServiceCollection();
            services.AddAssemblyAppVersion(typeof(VersionEnricherTests));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(serviceProvider)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched with a assembly version from a custom consumer type");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(VersionEnricher.DefaultPropertyName, key);
            Assert.Equal(AssemblyVersion, value.ToDecentString());
        }

        [Fact]
        public void LogEvent_WithCustomConsumerTypeWithoutAssemblyVersion_SkipsApplicationVersionEnrichment()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var services = new ServiceCollection();
            
            var stubbedType = new Mock<Type>();
            stubbedType.Setup(t => t.Assembly).Returns(Mock.Of<Assembly>());
            services.AddAssemblyAppVersion(stubbedType.Object);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(serviceProvider)
                    .CreateLogger();

            // Act
            logger.Information("This log event will not be enriched with a assembly version because custom consumer type doesn't have a assembly version attribute");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            Assert.Empty(emit.Properties);
        }

        [Fact]
        public void LogEvent_WithCustomRegisteredAppVersionAndCustomPropertyName_UsesCustomAppVersion()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var propertyName = $"property-name-{Guid.NewGuid()}";
            var expected = $"version-{Guid.NewGuid()}";
            var services = new ServiceCollection();
            services.AddAppVersion(provider => new StubAppVersion(expected));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(serviceProvider, propertyName)
                    .CreateLogger();

            // Act
            logger.Information("This log event will be enriched by the registered stubbed app version on the custom property name");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);
            Assert.Equal(propertyName, key);
            Assert.Equal(expected, value.ToDecentString());
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogEvent_WithCustomAppVersionReturningBlankVersion_DoesntAddVersionProperty(string version)
        {
            // Arrange
            var spy = new InMemoryLogSink();
            var stubAppVersion = new Mock<IAppVersion>();
            stubAppVersion.Setup(v => v.GetVersion()).Returns(version);
            ILogger logger =
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.WithVersion(stubAppVersion.Object)
                    .CreateLogger();

            // Act
            logger.Information("This log event will not be enriched with an custom application version because the version is blank");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            Assert.Empty(emit.Properties);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithVersion_WithBlankPropertyName_Throws(string propertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => configuration.Enrich.WithVersion(propertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithVersion_WithAppVersionAndBlankPropertyName_Throws(string propertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            var appVersion = Mock.Of<IAppVersion>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => configuration.Enrich.WithVersion(appVersion, propertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithVersion_WithServiceProviderAndBlankPropertyName_Throws(string propertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => configuration.Enrich.WithVersion(serviceProvider, propertyName));
        }

        [Fact]
        public void WithVersion_WithoutAppVersion_Throws()
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithVersion(appVersion: null));
        }

        [Fact]
        public void WithVersion_WithoutAppVersionAndPropertyName_Throws()
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithVersion(appVersion: null, propertyName: "some-property-name"));
        }

        [Fact]
        public void WithVersion_WithoutServiceProvider_Throws()
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithVersion(serviceProvider: null));
        }

        [Fact]
        public void WithVersion_WithoutServiceProviderAndPropertyName_Throws()
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithVersion(serviceProvider: null, propertyName: "some-property-name"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankPropertyName_Throws(string propertyName)
        {
            Assert.ThrowsAny<ArgumentException>(() => new VersionEnricher(propertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithAppVersionAndBlankPropertyName_Throws(string propertyName)
        {
            // Arrange
            var appVersion = Mock.Of<IAppVersion>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new VersionEnricher(appVersion, propertyName));
        }

        [Fact]
        public void CreateEnricher_WithoutAppVersion_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(() => new VersionEnricher(appVersion: null));
        }

        [Fact]
        public void CreateEnricher_WithoutAppVersionAndCustomPropertyName_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new VersionEnricher(appVersion: null, propertyName: "some-property-name"));
        }
    }
}
