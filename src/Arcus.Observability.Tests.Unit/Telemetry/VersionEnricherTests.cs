using System;
using System.Linq;
using Arcus.Observability.Telemetry.Serilog;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
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
            (string key, LogEventPropertyValue value) = Assert.Single(emit.Properties);

            Assert.Equal("version", key);
            Assert.True(Version.TryParse(value.ToString().Trim('\"'), out Version result));
        }
    }
}
