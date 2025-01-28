using System;
using System.Reflection;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters
{
    /// <summary>
    /// Represents a conversion from a Serilog <see cref="LogEvent"/> to an Application Insights <see cref="ExceptionTelemetry"/> instance.
    /// </summary>
    public class ExceptionTelemetryConverter : CustomTelemetryConverter<ExceptionTelemetry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTelemetryConverter" /> class.
        /// </summary>
        /// <param name="options">The user-defined configuration options to influence the behavior of the Application Insights Serilog sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is <c>null</c>.</exception>
        public ExceptionTelemetryConverter(ApplicationInsightsSinkOptions options) : base(options)
        {
        }
        
        /// <summary>
        ///     Creates a telemetry entry for a given log event
        /// </summary>
        /// <param name="logEvent">Event that was logged and written to this sink</param>
        /// <param name="formatProvider">Provider to format event</param>
        /// <returns>Telemetry entry to emit to Azure Application Insights</returns>
        protected override ExceptionTelemetry CreateTelemetryEntry(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent), "Requires a Serilog log event to create an exception telemetry entry");
            }
            if (logEvent.Exception is null)
            {
                throw new ArgumentException("Requires a log event that tracks an exception to create an exception telemetry entry", nameof(logEvent));
            }
            
            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception);

            if (Options?.Exception.IncludeProperties == true)
            {
                EnrichWithExceptionProperties(logEvent, exceptionTelemetry);
            }
            
            return exceptionTelemetry;
        }

        private void EnrichWithExceptionProperties(LogEvent logEvent, ExceptionTelemetry exceptionTelemetry)
        {
            Type exceptionType = logEvent.Exception.GetType();
            PropertyInfo[] exceptionProperties = exceptionType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            
            foreach (PropertyInfo exceptionProperty in exceptionProperties)
            {
                string propertyFormat = DeterminePropertyFormat();

                string key = String.Format(propertyFormat, exceptionProperty.Name);
                var value = exceptionProperty.GetValue(logEvent.Exception)?.ToString();
                exceptionTelemetry.Properties[key] = value;
            }
        }

        private string DeterminePropertyFormat()
        {
            if (Options != null)
            {
                return Options.Exception.PropertyFormat;
            }

            throw new InvalidOperationException(
                "Could not determine exception property format because the Application Insights exception converter was not initialized with any options");
        }
    }
}
