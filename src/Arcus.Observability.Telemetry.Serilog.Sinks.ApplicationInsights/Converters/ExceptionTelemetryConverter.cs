using System;
using System.Reflection;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using GuardNet;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="ExceptionTelemetry"/> instance.
    /// </summary>
    public class ExceptionTelemetryConverter : CustomTelemetryConverter<ExceptionTelemetry>
    {
        private readonly ApplicationInsightsSinkExceptionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The consumer-configurable options to influence how the exception should be tracked.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public ExceptionTelemetryConverter(ApplicationInsightsSinkExceptionOptions options)
        {
            Guard.NotNull(options, nameof(options), "Requires a set of user-configurable options to influence the behavior of how exceptions are tracked");
            _options = options;
        }
        
        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override ExceptionTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            Guard.NotNull(logEvent, nameof(logEvent), "Requires a Serilog log event to create an exception telemetry entry");
            Guard.For(() => logEvent.Exception is null, new ArgumentException(
                "Requires a log event that tracks an exception to create an exception telemetry entry", nameof(logEvent)));
            
            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception);

            Type exceptionType = logEvent.Exception.GetType();
            PropertyInfo[] exceptionProperties = exceptionType.GetProperties(BindingFlags.Public);
            foreach (PropertyInfo exceptionProperty in exceptionProperties)
            {
                string key = String.Format(_options.PropertyFormat, exceptionProperty.Name);
                var value = exceptionProperty.GetValue(logEvent.Exception)?.ToString();
                exceptionTelemetry.Properties[key] = value;
            }
            
            return exceptionTelemetry;
        }
    }
}
