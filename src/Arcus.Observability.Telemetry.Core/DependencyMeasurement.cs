using System;
using System.Diagnostics;

namespace Arcus.Observability.Telemetry.Core
{
    /// <summary>
    /// Represents an instance to measure easily dependencies in an application.
    /// </summary>
    public class DependencyMeasurement : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        
        private DependencyMeasurement(string dependencyData, DateTimeOffset startTime, Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;
            StartTime = startTime;
            DependencyData = dependencyData;
        }

        /// <summary>
        /// Starts measuring a dependency until the measurement is disposed.
        /// </summary>
        public static DependencyMeasurement Start()
        {
            return Start(dependencyData: null);
        }

        /// <summary>
        /// Starts measuring a dependency until the measurement is disposed.
        /// </summary>
        /// <param name="dependencyData">The additional data related to the dependency.</param>
        public static DependencyMeasurement Start(string dependencyData)
        {
            return new DependencyMeasurement(dependencyData, DateTimeOffset.UtcNow,  Stopwatch.StartNew());
        }

        /// <summary>
        /// Gets the time when the dependency measurement was started.
        /// </summary>
        public DateTimeOffset StartTime { get; } 

        /// <summary>
        /// Gets the total elapsed time measured for the dependency.
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// Gets the additional data that corresponds with the measured dependency.
        /// </summary>
        public string DependencyData { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
        }
    }
}
