using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture
{
    public class InMemoryApplicationInsightsTelemetryConverter : TelemetryConverterBase
    {
        private readonly ConcurrentStack<ITelemetry> _telemetries = new();

        public ApplicationInsightsSinkOptions Options { get; set; }

        public RequestTelemetry[] Requests => _telemetries.ToArray().OfType<RequestTelemetry>().ToArray();
        public DependencyTelemetry[] Dependencies => _telemetries.ToArray().OfType<DependencyTelemetry>().ToArray();
        public EventTelemetry[] Events => _telemetries.ToArray().OfType<EventTelemetry>().ToArray();
        public MetricTelemetry[] Metrics => _telemetries.ToArray().OfType<MetricTelemetry>().ToArray();
        public TraceTelemetry[] Traces => _telemetries.ToArray().OfType<TraceTelemetry>().ToArray();
        public ExceptionTelemetry[] Exceptions => _telemetries.ToArray().OfType<ExceptionTelemetry>().ToArray();

        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var converter = ApplicationInsightsTelemetryConverter.Create(Options);

            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider);
            foreach (ITelemetry telemetry in telemetries)
            {
                _telemetries.Push(telemetry);
            }

            return Enumerable.Empty<ITelemetry>();
        }
    }
}
