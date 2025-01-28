using System;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string instrumentationKey)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, serviceProvider, instrumentationKey, LogEventLevel.Verbose);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string instrumentationKey,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, serviceProvider, instrumentationKey, LogEventLevel.Verbose, configureOptions);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string instrumentationKey,
            LogEventLevel restrictedToMinimumLevel)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            return AzureApplicationInsightsWithInstrumentationKey(loggerSinkConfiguration, serviceProvider, instrumentationKey, restrictedToMinimumLevel, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="instrumentationKey">The required Application Insights key.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="instrumentationKey"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithInstrumentationKey(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string instrumentationKey,
            LogEventLevel restrictedToMinimumLevel,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            var options = new ApplicationInsightsSinkOptions();
            configureOptions?.Invoke(options);

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, serviceProvider, "InstrumentationKey=" + instrumentationKey, restrictedToMinimumLevel, configureOptions);
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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

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
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            var options = new ApplicationInsightsSinkOptions();
            configureOptions?.Invoke(options);

            var client = new TelemetryClient(TelemetryConfiguration.CreateDefault());
            client.TelemetryConfiguration.ConnectionString = connectionString;

            return loggerSinkConfiguration.ApplicationInsights(client, ApplicationInsightsTelemetryConverter.Create(options), restrictedToMinimumLevel);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string connectionString)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, serviceProvider, connectionString, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string connectionString,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, serviceProvider, connectionString, LogEventLevel.Verbose, configureOptions);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string connectionString,
            LogEventLevel restrictedToMinimumLevel)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }

            return AzureApplicationInsightsWithConnectionString(loggerSinkConfiguration, serviceProvider, connectionString, restrictedToMinimumLevel, configureOptions: null);
        }

        /// <summary>
        ///     Adds a Serilog sink that writes <see cref="T:Serilog.Events.LogEvent">log events</see> to Azure Application Insights.
        /// </summary>
        /// <remarks>
        ///     Supported telemetry types are Traces, Dependencies, Events, Requests and Metrics for which we provide extensions on <see cref="ILogger" />.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="serviceProvider">
        ///     The provider instance to retrieve the <see cref="TelemetryClient"/> in the application services.
        ///     Note that this is only required when the application requires W3C correlation.
        /// </param>
        /// <param name="connectionString">The required Application Insights connection string.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="configureOptions">The optional function to configure additional options to influence the behavior of how the telemetry is logged to Azure Application Insights.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="loggerSinkConfiguration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        public static LoggerConfiguration AzureApplicationInsightsWithConnectionString(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            IServiceProvider serviceProvider,
            string connectionString,
            LogEventLevel restrictedToMinimumLevel,
            Action<ApplicationInsightsSinkOptions> configureOptions)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration), "Requires a logger configuration to add the Azure Application Insights sink to");
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Requires an instrumentation key to authenticate with Azure Application Insights while sinking telemetry");
            }


            var options = new ApplicationInsightsSinkOptions();
            configureOptions?.Invoke(options);

            var client = serviceProvider.GetService<TelemetryClient>();
            if (client is null)
            {
                throw new InvalidOperationException(
                    "Could not retrieve Microsoft telemetry client from the application registered services, this happens when the Application Insights services are not registered in the application services," 
                    + "please use one of Arcus' extensions like 'services.AddHttpCorrelation()' to automatically register the Application Insights when using the W3C correlation system, "
                    + $"when using the Hierarchical correlation system, use the {nameof(AzureApplicationInsightsWithConnectionString)} extension without the service provider instead");
            }

            client.TelemetryConfiguration.ConnectionString = connectionString;

            return loggerSinkConfiguration.ApplicationInsights(client, ApplicationInsightsTelemetryConverter.Create(options), restrictedToMinimumLevel);
        }
    }
}