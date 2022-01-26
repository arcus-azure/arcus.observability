using System;
using System.Diagnostics;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents an instance to measure easily requests in an application.
    /// </summary>
    public class RequestMeasurement : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        private RequestMeasurement()
        {
            _stopwatch = Stopwatch.StartNew();
            StartTime = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Starts measuring a request until the measurement is disposed.
        /// </summary>
        public static RequestMeasurement Start()
        {
            return new RequestMeasurement();
        }

        /// <summary>
        /// Gets the time when the request measurement was started.
        /// </summary>
        public DateTimeOffset StartTime { get; }

        /// <summary>
        /// Gets the total elapsed time measured for the request.
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
        }
    }
}