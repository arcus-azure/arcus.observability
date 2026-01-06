using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using OpenTelemetry;

namespace Arcus.Observability.Telemetry.OpenTelemetry
{
    /// <summary>
    /// Represents the OpenTelemetry implementation of the <see cref="IObservability"/> interface
    /// to track telemetry throughout the application in a developer-friendly way.
    /// </summary>
    internal sealed class OpenTelemetryObservability : IObservability
    {
        private readonly ActivitySource _applicationSource;
        private readonly IMeterFactory _meterFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTelemetryObservability"/> class.
        /// </summary>
        internal OpenTelemetryObservability(ActivitySource applicationSource, IMeterFactory meterFactory)
        {
            ArgumentNullException.ThrowIfNull(applicationSource);
            ArgumentNullException.ThrowIfNull(meterFactory);

            _applicationSource = applicationSource;
            _meterFactory = meterFactory;
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
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="telemetryContext"/> is <c>null</c>.</exception>
        public RequestOperationResult StartCustomRequest(string operationName, IDictionary<string, object> telemetryContext)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
            ArgumentNullException.ThrowIfNull(telemetryContext);

            using var activity = _applicationSource.StartActivity(operationName, ActivityKind.Server);
            if (activity is null)
            {
                return new NoRequestOperationResult();
            }

            foreach (var item in telemetryContext)
            {
                activity.SetTag(item.Key, item.Value);
            }

            return new OpenTelemetryRequestOperationResult(activity);
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
            ArgumentException.ThrowIfNullOrWhiteSpace(dependencyName);
            ArgumentNullException.ThrowIfNull(telemetryContext);

            using var activity = _applicationSource.StartActivity(dependencyName, ActivityKind.Client);
            if (activity is null)
            {
                return new NoDependencyOperationResult();
            }

            foreach (var item in telemetryContext)
            {
                activity.SetTag(item.Key, item.Value);
            }

            return new OpenTelemetryDependencyOperationResult(activity);
        }

        /// <summary>
        /// Start building a metric in the application with the given <paramref name="metricName"/>.
        /// </summary>
        /// <param name="metricName">The name of the metric.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="metricName"/> is blank.</exception>
        public RecordedMetricBuilder RecordMetric(string metricName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(metricName);

            var meter = _meterFactory.Create(OpenTelemetryObservabilityOptions.GetDefaultMeterName());
            return new OpenTelemetryRecordedMetricBuilder(metricName, meter);
        }

        private sealed class OpenTelemetryRecordedMetricBuilder : RecordedMetricBuilder
        {
            private readonly string _metricName;
            private readonly Meter _meter;

            /// <summary>
            /// Initializes a new instance of the <see cref="RecordedMetricBuilder"/> class.
            /// </summary>
            public OpenTelemetryRecordedMetricBuilder(string metricName, Meter meter)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(metricName);
                ArgumentNullException.ThrowIfNull(meter);

                _metricName = metricName;
                _meter = meter;
            }

            /// <summary>
            /// Records the increment value of the measurement.
            /// </summary>
            /// <typeparam name="TValue">The numerical type of the measurement.</typeparam>
            /// <param name="delta">The increment measurement.</param>
            /// <param name="telemetryContext">The additional contextual information to provide context to the metric.</param>
            /// <exception cref="ArgumentNullException">Thrown when the <paramref name="telemetryContext"/> is <c>null</c>.</exception>
            public override void WithValue<TValue>(TValue delta, IDictionary<string, object> telemetryContext)
            {
                _meter.CreateCounter<TValue>(_metricName).Add(delta, telemetryContext.ToArray());
            }
        }

        private sealed class NoRequestOperationResult : RequestOperationResult
        {
            /// <summary>
            /// Finalizes the tracked request operation in the observability backend, based on the operation results.
            /// </summary>
            /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
            /// <param name="startTime">The date when the operation started.</param>
            /// <param name="duration">The time it took for the operation to run.</param>
            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
            }
        }

        private sealed class OpenTelemetryRequestOperationResult(Activity activity) : RequestOperationResult
        {
            /// <summary>
            /// Finalizes the tracked request operation in the observability backend, based on the operation results.
            /// </summary>
            /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
            /// <param name="startTime">The date when the operation started.</param>
            /// <param name="duration">The time it took for the operation to run.</param>
            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
                activity.SetStatus(isSuccessful ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                activity.Dispose();
            }
        }

        private sealed class NoDependencyOperationResult : DependencyOperationResult
        {
            /// <summary>
            /// Finalizes the tracked dependency operation in the observability backend, based on the operation results.
            /// </summary>
            /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
            /// <param name="startTime">The date when the operation started.</param>
            /// <param name="duration">The time it took for the operation to run.</param>
            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
            }
        }

        private sealed class OpenTelemetryDependencyOperationResult(Activity activity) : DependencyOperationResult
        {
            /// <summary>
            /// Finalizes the tracked dependency operation in the observability backend, based on the operation results.
            /// </summary>
            /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
            /// <param name="startTime">The date when the operation started.</param>
            /// <param name="duration">The time it took for the operation to run.</param>
            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
                activity.SetStatus(isSuccessful ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                activity.Dispose();
            }
        }
    }
}
