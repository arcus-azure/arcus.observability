using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Arcus.Observability.Tests.Core;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Enrichers
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

        [Fact]
        public void LogEvent_WithCustomComponentNamePropertyName_HasCustomComponentNamePropertyName()
        {
            // Arrange
            string expected = $"component-{Guid.NewGuid()}";
            string propertyName = $"component-property-name-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithComponentName(expected, propertyName)
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will be enriched with a custom property name which has the component name");

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(propertyName, expected),
                $"Log event should have custom property '{propertyName}' with component name '{expected}'");
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithComponentName_WithBlankPropertyName_Throws(string propertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithComponentName("some component name", propertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankPropertyName_Throws(string propertyName)
        {
            Assert.ThrowsAny<ArgumentException>(() => new ApplicationEnricher("some component name", propertyName));
        }
    }
}
