using System;
using System.Net.Http.Headers;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    public abstract class DependencyTelemetryConverter : CustomTelemetryConverter<DependencyTelemetry>
    {
        protected abstract DependencyType DependencyType { get; }

        protected override DependencyTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var target = logEvent.Properties.GetAsRawString(ContextProperties.DependencyTracking.TargetName);
            var dependencyName = logEvent.Properties.GetAsRawString(ContextProperties.DependencyTracking.DependencyName);
            var data = logEvent.Properties.GetAsRawString(ContextProperties.DependencyTracking.DependencyData);
            var startTime = logEvent.Properties.GetAsDateTimeOffset(ContextProperties.DependencyTracking.StartTime);
            var duration = logEvent.Properties.GetAsTimeSpan(ContextProperties.DependencyTracking.Duration);
            var resultCode = logEvent.Properties.GetAsRawString(ContextProperties.DependencyTracking.ResultCode);
            var outcome = logEvent.Properties.GetAsBool(ContextProperties.DependencyTracking.IsSuccessful);
            var operationId = logEvent.Properties.GetAsRawString(ContextProperties.Correlation.OperationId);

            var dependencyTelemetry = new DependencyTelemetry(DependencyType.ToString(), target, dependencyName, data, startTime, duration, resultCode, success: outcome)
            {
                Id = operationId
            };

            return dependencyTelemetry;
        }

        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.TargetName);
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.DependencyName);
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.DependencyData);
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.IsSuccessful);
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.ResultCode);
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.StartTime);
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.Duration);
        }
    }
}
