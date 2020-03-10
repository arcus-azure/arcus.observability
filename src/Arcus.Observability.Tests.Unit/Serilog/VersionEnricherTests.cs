using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
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
            var configuration = 
                new LoggerConfiguration()
                    .WriteTo.Sink(spy)
                    .Enrich.With<VersionEnricher>();

            ILogger logger = configuration.CreateLogger();

            // Act
            logger.Information("This log event should contain a 'version' property when written to the sink");

            // Assert
            LogEvent emit = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(emit);
            var property = Assert.Single(emit.Properties);
            Assert.Equal("version", property.Key);
            Assert.True(Version.TryParse(property.Value.ToString().Trim('\"'), out Version result));
        }
    }
}
