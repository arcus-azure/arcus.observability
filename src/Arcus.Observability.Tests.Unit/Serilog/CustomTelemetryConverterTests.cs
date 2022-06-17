using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class CustomTelemetryConverterTests
    {
        [Fact]
        public void CreateTraceConverter_WithoutOptions_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new TraceTelemetryConverter(options: null));
        }

        [Fact]
        public void CreateMetricConverter_WithoutOptions_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new MetricTelemetryConverter(options: null));
        }

        [Fact]
        public void CreateDependencyConverter_WithoutOptions_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new DependencyTelemetryConverter(options: null));
        }

        [Fact]
        public void CreateEventConverter_WithoutOptions_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new EventTelemetryConverter(options: null));
        }

        [Fact]
        public void CreateRequestConverter_WithoutOptions_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new RequestTelemetryConverter(options: (ApplicationInsightsSinkOptions) null));
        }

        [Fact]
        public void CreateExceptionConverter_WithoutOptions_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new ExceptionTelemetryConverter(options: (ApplicationInsightsSinkOptions) null));
        }
    }
}
