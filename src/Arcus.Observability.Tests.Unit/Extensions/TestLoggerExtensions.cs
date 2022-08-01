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
        /// Gets the written message to the <paramref name="logger"/> as a strongly-typed Dependency.
        /// </summary>
        /// <param name="logger">The test logger where a test message is written to.</param>
        /// <returns>
        ///     The strongly-typed Dependency containing the telemetry information.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no test message was written to the test <paramref name="logger"/>.</exception>
        public static DependencyLogEntry GetMessageAsDependency(this TestLogger logger)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a test logger to retrieve the written log message");

            if (logger.WrittenMessage is null)
            {
                throw new InvalidOperationException(
                    "Cannot parse the written message as a telemetry dependency because no log message was written to this test logger");
            }

            const string pattern = @"^(?<dependencytype>[\w\s]+) (?<dependencyname>[\w\s\.\/\/:\-]+)? (?<dependencydata>[\w\s\-\,\/]+)? named (?<targetname>[\w\s\.\/\/:\|\-]+)? with ID (?<dependencyid>[\w\s\-]*)? in (?<duration>(\d{1}\.)?\d{2}:\d{2}:\d{2}\.\d{7}) at (?<starttime>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{7} \+\d{2}:\d{2}) \(IsSuccessful: (?<issuccessful>(True|False)) - ResultCode: (?<resultcode>\d*) - Context: \{(?<context>((\[\w+, \w+\])(; \[[\w\-]+, \w+\])*))\}\)$";
            Match match = Regex.Match(logger.WrittenMessage, pattern);

            string dependencyType = match.GetGroupValue("dependencytype");
            string dependencyName = match.GetGroupValue("dependencyname");
            string dependencyData = match.GetGroupValue("dependencydata");
            string targetName = match.GetGroupValue("targetname");
            string dependencyId = match.GetGroupValue("dependencyid");
            TimeSpan duration = match.GetGroupValueAsTimeSpan("duration");
            DateTimeOffset startTime = match.GetGroupValueAsDateTimeOffset("starttime");
            bool isSuccessful = match.GetGroupValueAsBool("issuccessful");
            int? resultCode = match.GetGroupValueAsNullableInt("resultcode");
            IDictionary<string, object> context = match.GetGroupValueAsTelemetryContext("context", TelemetryType.Dependency);

            return new DependencyLogEntry(
                dependencyType,
                dependencyName,
                dependencyData,
                targetName,
                dependencyId,
                duration,
                startTime,
                resultCode,
                isSuccessful,
                context);
        }
        
        /// <summary>
        /// Gets the written message to the <paramref name="logger"/> as a strongly-typed Request.
        /// </summary>
        /// <param name="logger">The test logger where a test message is written to.</param>
        /// <returns>
        ///     The strongly-typed Request containing the telemetry information.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no test message was written to the test <paramref name="logger"/>.</exception>
        public static RequestLogEntry GetMessageAsRequest(this TestLogger logger)
        {
            Guard.NotNull(logger, nameof(logger), "Requires a test logger to retrieve the written log message");

            if (logger.WrittenMessage is null)
            {
                throw new InvalidOperationException(
                    "Cannot parse the written message as a telemetry request because no log message was written to this test logger");
            }

            const string httpRequestPattern = @"(?<method>(POST|GET|HEAD|PUT|DELETE)) (?<scheme>\w+):\/\/(?<host>\w+)\/(?<uri>\/\w+) from (?<operation>[\w\s\/]+) completed with (?<statuscode>[0-9]+) in (?<duration>(\d{1}\.)?\d{2}:\d{2}:\d{2}\.\d{7}) at (?<date>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{7} \+\d{2}:\d{2}) - \(Context: \{(?<context>((\[[\w\-]+, \w+\])(; \[[\w\-]+, [\w\.]+\])*))\}\)$";
            Match httpRequestMatch = Regex.Match(logger.WrittenMessage, httpRequestPattern);

            if (httpRequestMatch.Success)
            {
                string method = httpRequestMatch.GetGroupValue("method");
                string scheme = httpRequestMatch.GetGroupValue("scheme");
                string host = httpRequestMatch.GetGroupValue("host");
                string uri = httpRequestMatch.GetGroupValue("uri");
                string operationName = httpRequestMatch.GetGroupValue("operation");
                TimeSpan duration = httpRequestMatch.GetGroupValueAsTimeSpan("duration");
                DateTimeOffset startTime = httpRequestMatch.GetGroupValueAsDateTimeOffset("date");
                int statusCode = httpRequestMatch.GetGroupValueAsNullableInt("statuscode").GetValueOrDefault();
                IDictionary<string, object> context = httpRequestMatch.GetGroupValueAsTelemetryContext("context", TelemetryType.Request);

                return RequestLogEntry.CreateForHttpRequest(method, scheme, host, uri, operationName, statusCode, startTime, duration, context);
            }
            else
            {
                const string pattern = @"(Azure Service Bus|Azure EventHubs|Custom \w+) from (?<operationname>[\w\s]+) completed in (?<duration>(\d{1}\.)?\d{2}:\d{2}:\d{2}\.\d{7}) at (?<timestamp>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{7} \+\d{2}:\d{2}) - \(IsSuccessful: (?<issuccessful>(True|False)), Context: \{(?<context>((\[[\w\-]+, [\w\$]+\])(; \[[\w\-]+, [\w\$\.]+\])*))\}\)$";
                Match match = Regex.Match(logger.WrittenMessage, pattern);

                string operationName = match.GetGroupValue("operationname");
                TimeSpan duration = match.GetGroupValueAsTimeSpan("duration");
                DateTimeOffset startTime = match.GetGroupValueAsDateTimeOffset("timestamp");
                bool isSuccessful = match.GetGroupValueAsBool("issuccessful");
                IDictionary<string, object> context = match.GetGroupValueAsTelemetryContext("context", TelemetryType.Request);

                if (logger.WrittenMessage.StartsWith("Azure Service Bus"))
                {
                    return RequestLogEntry.CreateForServiceBus(operationName, isSuccessful, duration, startTime, context);
                }

                if (logger.WrittenMessage.StartsWith("Azure EventHubs"))
                {
                    return RequestLogEntry.CreateForEventHubs(operationName, isSuccessful, duration, startTime, context);
                }

                if (logger.WrittenMessage.StartsWith("Custom"))
                {
                    int length = logger.WrittenMessage.IndexOf(" from ", StringComparison.CurrentCulture);
                    int startIndex = "Custom ".Length;
                    string customRequestSource = logger.WrittenMessage.Substring(startIndex, length - startIndex);
                    return RequestLogEntry.CreateForCustomRequest(customRequestSource, operationName, isSuccessful, duration, startTime, context);
                }

                throw new InvalidOperationException("Cannot determine request source system during parsing of logged request");
            }
        }

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

            string metricName = match.GetGroupValue("metricname");
            double metricValue = match.GetGroupValueAsDouble("metricvalue");
            DateTimeOffset timestamp = match.GetGroupValueAsDateTimeOffset("timestamp");
            IDictionary<string, object> context = match.GetGroupValueAsTelemetryContext("context", TelemetryType.Metrics);

            return new MetricLogEntry(metricName, metricValue, timestamp, context);
        }

        private static string GetGroupValue(this Match match, string name)
        {
            Assert.True(
                match.Groups.TryGetValue(name, out Group group), 
                $"Cannot find {name} in logged message");

            return group.Captures.FirstOrDefault()?.Value;
        }

        private static double GetGroupValueAsDouble(this Match match, string name)
        {
            string value = match.GetGroupValue(name);
            
            Assert.True(
                double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double metricValue),
                $"Cannot parse {name} to double");
            
            return metricValue;
        }

        private static TimeSpan GetGroupValueAsTimeSpan(this Match match, string name)
        {
            string value = match.GetGroupValue(name);
            
            Assert.True(
                TimeSpan.TryParse(value, out TimeSpan span),
                $"Cannot parse {name} to a time span");

            return span;
        }

        private static DateTimeOffset GetGroupValueAsDateTimeOffset(this Match match, string name)
        {
            string value = match.GetGroupValue(name);
            
            Assert.True(
                DateTimeOffset.TryParse(value, out DateTimeOffset timestamp),
                $"Cannot parse {name} to date time offset");
           
            return timestamp;
        }

        private static bool GetGroupValueAsBool(this Match match, string name)
        {
            string value = match.GetGroupValue(name);
            
            Assert.True(
                bool.TryParse(value, out bool result),
                $"Cannot parse {name} to boolean");

            return result;
        }

        private static int? GetGroupValueAsNullableInt(this Match match, string name)
        {
            string value = match.GetGroupValue(name);

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return null;
        }

        private static IDictionary<string, object> GetGroupValueAsTelemetryContext(this Match match, string name, TelemetryType telemetryType)
        {
            string contextText = match.GetGroupValue(name);

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
                       && item.Value.ToString() == telemetryType.ToString();
            });
            context.Remove(ContextProperties.General.TelemetryType);
            
            return context;
        }
    }
}
