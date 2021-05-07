using System;
using System.Linq;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Filters;
using Bogus;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    [Trait("Category", "Unit")]
    public class TelemetryTypeFilterTests
    {
        private readonly Faker _bogusGenerator = new Faker();
        
        [Fact]
        public void LogEventAsTelemetry_FiltersInCorrectTelemetry_Succeeds()
        {
            // Arrange
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();
            var level = _bogusGenerator.Random.Enum<LogEventLevel>();
            var telemetryType = _bogusGenerator.Random.Enum<TelemetryType>();
            var properties = new[]
            {
                new LogEventProperty(ContextProperties.General.TelemetryType, new ScalarValue(telemetryType))
            };

            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, properties);
            var filter = TelemetryTypeFilter.On(telemetryType, isTrackingEnabled: true);
            
            // Act
            bool isEnabled = filter.IsEnabled(logEvent);
            
            // Assert
            Assert.True(isEnabled);
        }

        [Fact]
        public void LogEventAsTelemetry_FiltersOutCorrectTelemetry_Succeeds()
        {
            // Arrange
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();
            var level = _bogusGenerator.Random.Enum<LogEventLevel>();
            var telemetryType = _bogusGenerator.Random.Enum<TelemetryType>();
            var properties = new[]
            {
                new LogEventProperty(ContextProperties.General.TelemetryType, new ScalarValue(telemetryType))
            };

            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, properties);
            var filter = TelemetryTypeFilter.On(telemetryType);
            
            // Act
            bool isEnabled = filter.IsEnabled(logEvent);
            
            // Assert
            Assert.False(isEnabled);
        }

        [Fact]
        public void LogEventWithoutTelemetry_DoesntFilterAnything_Succeeds()
        {
            // Arrange
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();
            var level = _bogusGenerator.Random.Enum<LogEventLevel>();
            var telemetryType = _bogusGenerator.Random.Enum<TelemetryType>();
            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, Enumerable.Empty<LogEventProperty>());
            var filter = TelemetryTypeFilter.On(telemetryType);
            
            // Act
            bool isEnabled = filter.IsEnabled(logEvent);
            
            // Assert
            Assert.True(isEnabled);
        }

        [Fact]
        public void LogEventWithoutValidTelemetry_DoesntFilterAnything_Succeeds()
        {
            // Assert
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();
            var level = _bogusGenerator.Random.Enum<LogEventLevel>();
            var telemetryType = _bogusGenerator.Random.Enum<TelemetryType>();
            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, Enumerable.Empty<LogEventProperty>());
            var filter = TelemetryTypeFilter.On(telemetryType);
            
            // Act
            bool isEnabled = filter.IsEnabled(logEvent);
            
            // Assert
            Assert.True(isEnabled);
        }

        [Theory]
        [InlineData(TelemetryType.Request | TelemetryType.Dependency)]
        [InlineData(TelemetryType.Metrics | TelemetryType.Request | TelemetryType.Events)]
        public void CreateFilter_WithTelemetryTypeOutsideEnum_Fails(TelemetryType telemetryType)
        {
            Assert.ThrowsAny<ArgumentException>(() => TelemetryTypeFilter.On(telemetryType));
        }
        
        [Theory]
        [InlineData(TelemetryType.Request | TelemetryType.Dependency)]
        [InlineData(TelemetryType.Metrics | TelemetryType.Request | TelemetryType.Events)]
        public void CreateFilterWithTracked_WithTelemetryTypeOutsideEnum_Fails(TelemetryType telemetryType)
        {
            // Arrange
            bool isTrackingEnabled = _bogusGenerator.Random.Bool();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => TelemetryTypeFilter.On(telemetryType, isTrackingEnabled));
        }
    }
}
