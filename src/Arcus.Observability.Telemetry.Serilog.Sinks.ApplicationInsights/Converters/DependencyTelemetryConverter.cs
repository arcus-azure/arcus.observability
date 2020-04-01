using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an <see cref="DependencyTelemetry"/> instance.
    /// </summary>
    public abstract class DependencyTelemetryConverter : CustomTelemetryConverter<DependencyTelemetry>
    {
        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
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
            string dependencyType = GetDependencyType(logEvent);

            var dependencyTelemetry = new DependencyTelemetry(dependencyType, target, dependencyName, data, startTime, duration, resultCode, success: outcome)
            {
                Id = operationId
            };

            return dependencyTelemetry;
        }

        /// <summary>
        ///     Gets the custom dependency type name from the given <paramref name="logEvent"/> to use in an <see cref="DependencyTelemetry"/> instance.
        /// </summary>
        /// <param name="logEvent">The logged event.</param>
        protected abstract string GetDependencyType(LogEvent logEvent);

        /// <summary>
        ///     Provides capability to remove intermediary properties that are logged, but should not be tracked in the sink
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        protected override void RemoveIntermediaryProperties(LogEvent logEvent)
        {
            logEvent.RemovePropertyIfPresent(ContextProperties.DependencyTracking.DependencyType);
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
