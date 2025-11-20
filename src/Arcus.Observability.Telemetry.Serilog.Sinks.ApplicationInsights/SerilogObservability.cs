using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Represents the Serilog-based implementation of <see cref="IObservability"/> interface
    /// to track telemetry throughout the application in a developer-friendly way.
    /// </summary>
    internal class SerilogObservability : IObservability
    {
        private readonly ILogger<SerilogObservability> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogObservability"/> class.
        /// </summary>
        public SerilogObservability(ILogger<SerilogObservability> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        /// <summary>
        /// Tracks a custom request with contextual information to the configured observability backend.
        /// </summary>
        /// <remarks>
        ///     Make sure to dispose the returned operation result to flush the telemetry to the observability backend.
        /// </remarks>
        /// <param name="operationName">The name of the request operation.</param>
        /// <param name="telemetryContext">The additional contextual information to provide context to the request.</param>
        /// <returns>
        ///     The scoped request operation that allows to specify whether the request operation was successful.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationName"/> is blank.</exception>
        public RequestOperationResult StartCustomRequest(string operationName, IDictionary<string, object> telemetryContext)
        {
            return new SerilogRequestOperationResult(_logger, operationName, telemetryContext);
        }

        /// <summary>
        /// Tracks a custom dependency with contextual information to the configured observability backend.
        /// </summary>
        /// <param name="dependencyName">The name of the dependency operation.</param>
        /// <param name="telemetryContext">The additional contextual information to provide context to the dependency.</param>
        /// <returns>
        ///     The scoped dependency operation that allows to specify whether the dependency operation was successful.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyName"/> is blank.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="telemetryContext"/> is <c>null</c>.</exception>
        public DependencyOperationResult StartCustomDependency(string dependencyName, IDictionary<string, object> telemetryContext)
        {
            return new SerilogDependencyOperationResult(_logger, dependencyName, telemetryContext);
        }

        private sealed class SerilogRequestOperationResult(ILogger logger, string operationName, IDictionary<string, object> telemetryContext) : RequestOperationResult
        {
            /// <summary>
            /// Finalizes the tracked request operation in the observability backend, based on the operation results.
            /// </summary>
            /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
            /// <param name="startTime">The date when the operation started.</param>
            /// <param name="duration">The time it took for the operation to run.</param>
            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
                logger.LogCustomRequest("custom", operationName, isSuccessful, startTime, duration, new Dictionary<string, object>(telemetryContext));
            }
        }

        private sealed class SerilogDependencyOperationResult(ILogger logger, string dependencyName, IDictionary<string, object> telemetryContext) : DependencyOperationResult
        {
            /// <summary>
            /// Finalizes the tracked dependency operation in the observability backend, based on the operation results.
            /// </summary>
            /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
            /// <param name="startTime">The date when the operation started.</param>
            /// <param name="duration">The time it took for the operation to run.</param>
            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
                logger.LogDependency(
                    dependencyType: "custom",
                    dependencyData: "n/a",
                    isSuccessful: isSuccessful,
                    dependencyName: dependencyName,
                    startTime,
                    duration,
                    new Dictionary<string, object>(telemetryContext));
            }
        }

        /// <summary>
        /// Start building a metric in the application with the given <paramref name="metricName"/>.
        /// </summary>
        /// <param name="metricName">The name of the metric.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="metricName"/> is blank.</exception>
        public RecordedMetricBuilder RecordMetric(string metricName)
        {
            return new SerilogRecordedMetricBuilder(_logger, metricName);
        }

        private sealed class SerilogRecordedMetricBuilder(ILogger logger, string metricName) : RecordedMetricBuilder
        {
            /// <summary>
            /// Records the increment value of the measurement.
            /// </summary>
            /// <typeparam name="TValue">The numerical type of the measurement.</typeparam>
            /// <param name="delta">The increment measurement.</param>
            /// <param name="telemetryContext">The additional contextual information to provide context to the metric.</param>
            /// <exception cref="ArgumentNullException">Thrown when the <paramref name="telemetryContext"/> is <c>null</c>.</exception>
            public override void WithValue<TValue>(TValue delta, IDictionary<string, object> telemetryContext)
            {
                logger.LogCustomMetric(metricName, Convert.ToDouble(delta), new Dictionary<string, object>(telemetryContext));
            }
        }
    }
}
