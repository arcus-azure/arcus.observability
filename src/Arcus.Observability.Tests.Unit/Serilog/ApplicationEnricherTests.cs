using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class ApplicationEnricherTests
    {
        private const string ComponentName = "ComponentName";

        [Fact]
        public void LogEvent_WithApplicationEnricher_HasComponentName()
        {
            // Arrange
            string componentName = $"component-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithComponentName(componentName)
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will be enriched with application information"); 

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ComponentName, componentName),
                "Log event should contain component name property");
        }

        [Fact]
        public void LogEventWithComponentNameProperty_WithApplicationEnricher_DoesntAlterComponentNameProperty()
        {
            // Arrange
            string expectedComponentName = $"expected-component-{Guid.NewGuid()}";
            string ignoredComponentName = $"ignored-component-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With(new ApplicationEnricher(ignoredComponentName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will not be enriched with component name because it already has one called '{ComponentName}'", expectedComponentName);

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ComponentName, expectedComponentName),
                "Log event should not overwrite component name property");
        }
    }
}
