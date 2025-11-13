using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Arcus.Observability
{
    /// <summary>
    /// Represents a high-level facade that encapsulates low-level telemetry operations throughout the application.
    /// </summary>
    public interface IObservability
    {
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
        RequestOperationResult StartCustomRequest(string operationName, IDictionary<string, object> telemetryContext);

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
        DependencyOperationResult StartCustomDependency(string dependencyName, IDictionary<string, object> telemetryContext);

        /// <summary>
        /// Start building a metric in the application with the given <paramref name="metricName"/>.
        /// </summary>
        /// <param name="metricName">The name of the metric.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="metricName"/> is blank.</exception>
        RecordedMetricBuilder RecordMetric(string metricName);
    }

    /// <summary>
    /// Extensions on the <see cref="IObservability"/> interface for a more dev-friendly experience.
    /// </summary>
    public static class IObservabilityExtensions
    {
        /// <summary>
        /// Tracks a custom request with contextual information to the configured observability backend.
        /// </summary>
        /// <remarks>
        ///     Make sure to dispose the returned operation result to flush the telemetry to the observability backend.
        /// </remarks>
        /// <param name="observability">The observability instance to track the custom request.</param>
        /// <param name="operationName">The name of the request operation.</param>
        /// <returns>
        ///     The scoped request operation that allows to specify whether the request operation was successful.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="observability"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationName"/> is blank.</exception>
        public static RequestOperationResult StartCustomRequest(this IObservability observability, string operationName)
        {
            ArgumentNullException.ThrowIfNull(observability);
            return observability.StartCustomRequest(operationName, new Dictionary<string, object>());
        }

        /// <summary>
        /// Tracks a custom dependency with contextual information to the configured observability backend.
        /// </summary>
        /// <param name="observability">The observability instance to track the custom dependency.</param>
        /// <param name="dependencyName">The name of the dependency operation.</param>
        /// <returns>
        ///     The scoped dependency operation that allows to specify whether the dependency operation was successful.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyName"/> is blank.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="observability"/> is <c>null</c>.</exception>
        public static DependencyOperationResult StartCustomDependency(this IObservability observability, string dependencyName)
        {
            ArgumentNullException.ThrowIfNull(observability);
            return observability.StartCustomDependency(dependencyName, new Dictionary<string, object>());
        }
    }

    /// <summary>
    /// Represents the instance to build a metric in the application that can be tracked.
    /// </summary>
    public abstract class RecordedMetricBuilder
    {
        /// <summary>
        /// Records the increment value of the measurement.
        /// </summary>
        /// <typeparam name="TValue">The numerical type of the measurement.</typeparam>
        /// <param name="delta">The increment measurement.</param>
        /// <param name="telemetryContext">The additional contextual information to provide context to the metric.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="telemetryContext"/> is <c>null</c>.</exception>
        public abstract void WithValue<TValue>(TValue delta, IDictionary<string, object> telemetryContext) where TValue : struct;
    }

    /// <summary>
    /// Extensions on the <see cref="RecordedMetricBuilder"/> for a more dev-friendly experience.
    /// </summary>
    public static class RecordedMetricBuilderExtensions
    {
        /// <summary>
        /// Records the increment value of the measurement.
        /// </summary>
        /// <typeparam name="TValue">The numerical type of the measurement.</typeparam>
        /// <param name="builder">The metrics builder to track the metric value.</param>
        /// <param name="delta">The increment measurement.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> is <c>null</c>.</exception>
        public static void WithValue<TValue>(this RecordedMetricBuilder builder, TValue delta) where TValue : struct
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.WithValue(delta, new Dictionary<string, object>());
        }
    }

    /// <summary>
    /// Represents a result of an operation that can be used to track the success or failure of a request operation.
    /// </summary>
    public abstract class RequestOperationResult : IDisposable
    {
        private readonly Stopwatch _watch;
        private readonly DateTimeOffset _startTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestOperationResult" /> class.
        /// </summary>
        protected RequestOperationResult()
        {
            _startTime = DateTimeOffset.UtcNow;
            _watch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets or sets the boolean flag that indicates whether the request operation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            _watch.Stop();
            StopOperation(IsSuccessful, _startTime, _watch.Elapsed);
        }

        /// <summary>
        /// Finalizes the tracked request operation in the observability backend, based on the operation results.
        /// </summary>
        /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
        /// <param name="startTime">The date when the operation started.</param>
        /// <param name="duration">The time it took for the operation to run.</param>
        protected abstract void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration);
    }

    /// <summary>
    /// Represents a result of an operation that can be used to track the success or failure of a dependency operation.
    /// </summary>
    public abstract class DependencyOperationResult : IDisposable
    {
        private readonly Stopwatch _watch;
        private readonly DateTimeOffset _startTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyOperationResult" /> class.
        /// </summary>
        protected DependencyOperationResult()
        {
            _startTime = DateTimeOffset.UtcNow;
            _watch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets or sets the boolean flag that indicates whether the dependency operation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            _watch.Stop();
            StopOperation(IsSuccessful, _startTime, _watch.Elapsed);
        }

        /// <summary>
        /// Finalizes the tracked dependency operation in the observability backend, based on the operation results.
        /// </summary>
        /// <param name="isSuccessful">The boolean flag to indicate whether the operation was successful.</param>
        /// <param name="startTime">The date when the operation started.</param>
        /// <param name="duration">The time it took for the operation to run.</param>
        protected abstract void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration);
    }
}
