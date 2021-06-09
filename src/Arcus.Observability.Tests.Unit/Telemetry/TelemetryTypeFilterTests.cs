using System;
using System.Collections.Generic;
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

        [Theory]
        [InlineData(TelemetryType.Dependency, ContextProperties.DependencyTracking.DependencyLogEntry)]
        [InlineData(TelemetryType.Request, ContextProperties.RequestTracking.RequestLogEntry)]
        [InlineData(TelemetryType.Events, ContextProperties.EventTracking.EventLogEntry)]
        [InlineData(TelemetryType.Metrics, ContextProperties.MetricTracking.MetricLogEntry)]
        public void LogEventAsTelemetry_FiltersInCorrectTelemetry_Succeeds(TelemetryType telemetryType, string logEntryKey)
        {
            // Arrange
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();
            var level = _bogusGenerator.Random.Enum<LogEventLevel>();
            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, new []
            {
                new LogEventProperty(logEntryKey, new ScalarValue("something that represents the telemetry"))
            });
            var filter = TelemetryTypeFilter.On(telemetryType);
            
            // Act
            bool isEnabled = filter.IsEnabled(logEvent);
            
            // Assert
            Assert.True(isEnabled);
        }
        
        [Fact]
        public void LogEventAsTelemetry_FiltersInAllTelemetry_Succeeds()
        {
            // Arrange
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();
            var level = _bogusGenerator.Random.Enum<LogEventLevel>();
            var telemetryType = _bogusGenerator.Random.Enum<TelemetryType>();
            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, Enumerable.Empty<LogEventProperty>());
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
            var logEvent = new LogEvent(timestamp, level, exception: null, MessageTemplate.Empty, Enumerable.Empty<LogEventProperty>());
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
            Assert.False(isEnabled);
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
