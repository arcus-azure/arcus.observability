using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Telemetry extensions on the <see cref="ILogger"/> instance to write Application Insights compatible log messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Logs a custom metric
        /// </summary>
        /// <param name="logger">The logger to track the metric.</param>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="context">Context that provides more insights on the event that occurred</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public static void LogCustomMetric(this ILogger logger, string name, double value, Dictionary<string, object> context = null)
            => LogCustomMetric(logger, name, value, DateTimeOffset.UtcNow, context);

        /// <summary>
        /// Logs a custom metric
        /// </summary>
        /// <param name="logger">The logger to track the metric.</param>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="timestamp">Timestamp of the metric</param>
        /// <param name="context">Context that provides more insights on the event that occurred</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public static void LogCustomMetric(this ILogger logger, string name, double value, DateTimeOffset timestamp, Dictionary<string, object> context = null)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger), "Requires a logger instance to track telemetry");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Requires a non-blank name to track a metric", nameof(name));
            }

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.MetricFormat, new MetricLogEntry(name, value, timestamp, context));
        }
    }
}
