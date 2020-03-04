using System.Collections.Concurrent;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    /// <summary>
    /// Represents a logging sink that collects the emitted log events in-memory.
    /// </summary>
    public class InMemoryLogSink : ILogEventSink
    {
        private readonly ConcurrentQueue<LogEvent> _logEmits = new ConcurrentQueue<LogEvent>();

        /// <summary>
        /// Gets the current log emits available on the sink.
        /// </summary>
        public IEnumerable<LogEvent> CurrentLogEmits => _logEmits.ToArray();

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            _logEmits.Enqueue(logEvent);
        }
    }
}
