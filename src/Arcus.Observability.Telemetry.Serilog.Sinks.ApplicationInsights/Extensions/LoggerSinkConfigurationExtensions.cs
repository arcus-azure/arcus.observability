using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using GuardNet;
using Serilog.Events;

// ReSharper disable once CheckNamespace
namespace Serilog.Configuration
{
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application
        ///     Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on
        ///     <see cref="ILogger" />
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">Required Application Insights key.</param>
        /// <returns></returns>
        public static LoggerConfiguration AzureApplicationInsights(this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey)
        {
            return AzureApplicationInsights(loggerSinkConfiguration, instrumentationKey, LogEventLevel.Verbose);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application
        ///     Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests & Metrics for which we provide extensions on
        ///     <see cref="ILogger" />
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">Required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        public static LoggerConfiguration AzureApplicationInsights(this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey, LogEventLevel restrictedToMinimumLevel)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration));
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey));

            return loggerSinkConfiguration.ApplicationInsights(instrumentationKey, ApplicationInsightsTelemetryConverter.Create(), restrictedToMinimumLevel);
        }
    }
}