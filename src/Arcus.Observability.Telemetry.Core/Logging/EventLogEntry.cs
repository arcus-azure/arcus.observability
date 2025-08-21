using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcus.Observability.Telemetry.Core.Logging
{
    /// <summary>
    /// Represents an event as a log entry.
    /// </summary>
    public class EventLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogEntry" /> class.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="context">The context that provides more insights on the event that occurred.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public EventLogEntry(string name, IDictionary<string, object> context)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            EventName = name;
            Context = context ?? new Dictionary<string, object>();
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Events;
        }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Gets the context that provides more insights on the event that occurred.
        /// </summary>
        public IDictionary<string, object> Context { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string contextFormatted = $"{{{String.Join("; ", Context.Select(item => $"[{item.Key}, {item.Value}]"))}}}";
            return $"{EventName} (Context: {contextFormatted})";
        }
    }
}
