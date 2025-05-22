using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Arcus.Observability.Telemetry.Core.Logging
{
    /// <summary>
    /// Represents a metric as a log entry.
    /// </summary>
    public class MetricLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricLogEntry"/> class.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value of the metric.</param>
        /// <param name="timestamp">The timestamp of the metric.</param>
        /// <param name="context">The context that provides more insights on the event that occurred.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public MetricLogEntry(string name, double value, DateTimeOffset timestamp, IDictionary<string, object> context)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Requires a non-blank name to track a metric", nameof(name));
            }
            
            MetricName = name;
            MetricValue = value;
            Timestamp = timestamp.ToString(FormatSpecifiers.InvariantTimestampFormat);
            Context = context ?? new Dictionary<string, object>();
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Metrics;
        }

        /// <summary>
        /// Gets the name of the metric.
        /// </summary>
        public string MetricName { get; }
        
        /// <summary>
        /// Gets the value of the metric.
        /// </summary>
        public double MetricValue { get; }
        
        /// <summary>
        /// Gets the timestamp of the metric.
        /// </summary>
        public string Timestamp { get; }
        
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
            return $"{MetricName}: {MetricValue.ToString(CultureInfo.InvariantCulture)} at {Timestamp} (Context: {contextFormatted})";
        }
    }
}
