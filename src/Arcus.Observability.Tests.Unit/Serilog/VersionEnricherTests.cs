using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait(name: "Category", value: "Unit")]
    public class VersionEnricherTests
    {
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
        public void CreatesEnricher_WithBlankPropertyName_Throws(string propertyName)
        {
            Assert.ThrowsAny<ArgumentException>(() => new VersionEnricher(propertyName));
        }
    }
}
