using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Arcus.Observability
{
    /// <summary>
    /// Represents a generic <see cref="ILogger{TCategoryName}"/> that provides additional telemetry tracking
    /// besides standard logging capabilities.
    /// </summary>
    /// <typeparam name="TCategoryName">The type whose name is used for the logger category name.</typeparam>
    public interface ITelemetryLogger<out TCategoryName> : ILogger<TCategoryName>
    {
        /// <summary>
        /// Tracks a custom metric with contextual information to the configured observability backend.
        /// </summary>
        /// <typeparam name="TValue">The numerical unit of the metric value.</typeparam>
        /// <param name="metricName">The name of the custom metric.</param>
        /// <param name="metricValue">The current value of the custom metric.</param>
        /// <param name="telemetryContext">The additional information to provide context to the metric.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="metricName"/> is blank.</exception>
        void LogCustomMetric<TValue>(string metricName, TValue metricValue, IDictionary<string, object> telemetryContext) where TValue : struct;

        /// <summary>
        /// Tracks a custom event with contextual information to the configured observability backend.
        /// </summary>
        /// <param name="eventName">The name of the custom event.</param>
        /// <param name="timestamp">The time when the event occurred.</param>
        /// <param name="telemetryContext">The additional information to provide context to the event.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="eventName"/> is blank.</exception>
        void LogCustomEvent(string eventName, DateTimeOffset timestamp, IDictionary<string, object> telemetryContext);

        /// <summary>
        /// Tracks a custom dependency with contextual information to the configured observability backend.
        /// </summary>
        /// <param name="dependencyName">The name of the custom dependency.</param>
        /// <param name="isSuccessful">The boolean flag to indicate whether the interaction with the dependency was successful.</param>
        /// <param name="startTime">The time when the dependency was contacted.</param>
        /// <param name="duration">The time it took for the dependency to respond or complete the request.</param>
        /// <param name="telemetryContext">The additional contextual information to provide context to the dependency.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyName"/> is blank.</exception>
        void LogCustomDependency(string dependencyName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, IDictionary<string, object> telemetryContext);

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
        RequestOperationResult LogCustomRequest(string operationName, IDictionary<string, object> telemetryContext);
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
}
