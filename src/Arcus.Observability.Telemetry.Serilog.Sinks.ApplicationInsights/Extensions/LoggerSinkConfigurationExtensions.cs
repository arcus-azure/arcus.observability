using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using GuardNet;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Events;

// ReSharper disable once CheckNamespace
namespace Serilog.Configuration
{
    /// <summary>
    /// Extensions on the <see cref="LoggerSinkConfiguration"/> class to add the Azure Application Insights as a Serilog sink.
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        ///     See also synonym: <see cref="AzureApplicationInsightsWithInstrumentationKey(LoggerSinkConfiguration,string)"/>
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        [Obsolete("Use the " + nameof(AzureApplicationInsightsWithInstrumentationKey) + " overload instead")]
        public static LoggerConfiguration AzureApplicationInsights(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        ///     See also synonym: <see cref="AzureApplicationInsightsWithInstrumentationKey(LoggerSinkConfiguration,string,Action{ApplicationInsightsSinkOptions})"/>
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        [Obsolete("Use the " + nameof(AzureApplicationInsightsWithInstrumentationKey) + " overload instead")]
        public static LoggerConfiguration AzureApplicationInsights(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, LogEventLevel.Verbose, configureOptions);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        ///     See also synonym: <see cref="AzureApplicationInsightsWithInstrumentationKey(LoggerSinkConfiguration,string,LogEventLevel)"/>
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        [Obsolete("Use the " + nameof(AzureApplicationInsightsWithInstrumentationKey) + " overload instead")]
        public static LoggerConfiguration AzureApplicationInsights(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey, 
            LogEventLevel restrictedToMinimumLevel)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, restrictedToMinimumLevel, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        ///     See also synonym: <see cref="AzureApplicationInsightsWithInstrumentationKey(LoggerSinkConfiguration,string,LogEventLevel,Action{ApplicationInsightsSinkOptions})"/>
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        [Obsolete("Use the " + nameof(AzureApplicationInsightsWithInstrumentationKey) + " overload instead")]
        public static LoggerConfiguration AzureApplicationInsights(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey,
            LogEventLevel restrictedToMinimumLevel,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, restrictedToMinimumLevel, configureOptions);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, LogEventLevel.Verbose);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, LogEventLevel.Verbose, configureOptions);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey,
            LogEventLevel restrictedToMinimumLevel)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, instrumentationKey, restrictedToMinimumLevel, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string instrumentationKey,
            LogEventLevel restrictedToMinimumLevel,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(instrumentationKey, nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            var options = new ApplicationInsightsSinkOptions();
            configureOptions?.Invoke(options);

            return loggerSinkConfiguration.ApplicationInsights("InstrumentationKey=" + instrumentationKey, ApplicationInsightsTelemetryConverter.Create(options), restrictedToMinimumLevel);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string connectionString)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, connectionString, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string connectionString,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, connectionString, LogEventLevel.Verbose, configureOptions);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string connectionString,
            LogEventLevel restrictedToMinimumLevel)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, connectionString, restrictedToMinimumLevel, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string connectionString,
            LogEventLevel restrictedToMinimumLevel,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            Guard.NotNull(loggerSinkConfiguration, nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");

            var options = new ApplicationInsightsSinkOptions();
            configureOptions?.Invoke(options);

            var client = new TelemetryClient(TelemetryConfiguration.CreateDefault());
            client.TelemetryConfiguration.ConnectionString = connectionString;

            return loggerSinkConfiguration.ApplicationInsights(client, ApplicationInsightsTelemetryConverter.Create(options), restrictedToMinimumLevel);
        }
    }
}