using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using GuardNet;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Arcus.Observability.Tests.Unit
{
    /// <summary>
    /// Represents telemetry-specific extensions on the <see cref="TestLogger"/>.
    /// </summary>
    public static class TestLoggerExtensions
    {
        /// <summary>
        /// Gets the written message to the <paramref name="logger"/> as a strongly-typed Metric.
        /// </summary>
        /// <param name="logger">The test logger where a test message is written to.</param>
        /// <returns>
        ///     The strongly-typed Metric containing the telemetry information.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no test message was written to the test <paramref name="logger"/>.</exception>
        public static MetricLogEntry GetMessageAsMetric(this TestLogger logger)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a test logger to retrieve the written log message");

            if (logger.WrittenMessage is null)
            {
                throw new InvalidOperationException(
                    "Cannot parse the written message as a telemetry metric because no log message was written to this test logger");
            }

            const string pattern = @"^(?<metricname>[\w\s]+): (?<metricvalue>(0.\d+)) at (?<timestamp>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{7} \+\d{2}:\d{2}) \(Context: \{(?<context>((\[\w+, \w+\])(; \[\w+, \w+\])*))\}\)$";
            Match match = Regex.Match(logger.WrittenMessage, pattern);

            string metricName = GetMetricName(match);
            double metricValue = GetMetricValue(match);
            DateTimeOffset timestamp = GetTimestamp(match);
            IDictionary<string, object> context = GetTelemetryContext(match);

            return new MetricLogEntry(metricName, metricValue, timestamp, context);
        }

        private static string GetMetricName(Match match)
        {
            Assert.True(
                match.Groups.TryGetValue("metricname", out Group metricNameGroup),
                "Can't find metric name in logged message");
            Assert.NotNull(metricNameGroup);
            
            string metricName = Assert.Single(metricNameGroup.Captures).Value;
            return metricName;
        }

        private static double GetMetricValue(Match match)
        {
            Assert.True(
                match.Groups.TryGetValue("metricvalue", out Group metricValueGroup), 
                "Can't find metric value in logged message");
            
            Assert.NotNull(metricValueGroup);
            string metricValueText = Assert.Single(metricValueGroup.Captures).Value;
            
            Assert.True(
                double.TryParse(metricValueText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double metricValue),
                "Can't parse metric value to double");
            
            return metricValue;
        }

        private static DateTimeOffset GetTimestamp(Match match)
        {
            Assert.True(
                match.Groups.TryGetValue("timestamp", out Group timestampGroup), 
                "Can't timestamp in logged message");
            
            Assert.NotNull(timestampGroup);
            string timestampText = Assert.Single(timestampGroup.Captures).Value;
            
            Assert.True(
                DateTimeOffset.TryParse(timestampText, out DateTimeOffset timestamp),
                "Can't parse timestamp to date time offset");
           
            return timestamp;
        }

        private static IDictionary<string, object> GetTelemetryContext(Match match)
        {
            Assert.True(match.Groups.TryGetValue("context", out Group contextGroup), "Can't find context in logged message");
            Assert.NotNull(contextGroup);
            string contextText = Assert.Single(contextGroup.Captures).Value;

            char[] trailingChars = {'[', ']', ' '};
            IDictionary<string, object> context =
                contextText.Split(';')
                           .Select(item => item.Split(','))
                           .ToDictionary(
                               item => item[0].Trim(trailingChars),
                               item => (object) item[1].Trim(trailingChars));

            Assert.Single(context, item =>
            {
                return item.Key == ContextProperties.General.TelemetryType
                       && item.Value.ToString() == TelemetryType.Metrics.ToString();
            });
            context.Remove(ContextProperties.General.TelemetryType);
            
            return context;
        }
    }
}
