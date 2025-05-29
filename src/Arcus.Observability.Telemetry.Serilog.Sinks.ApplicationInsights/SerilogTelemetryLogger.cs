using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GuardNet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
{
    /// <summary>
    /// Represents an <see cref="ITelemetryLogger{TCategoryName}"/> implementation that uses Serilog to log telemetry entries to Azure Application Insights.
    /// </summary>
    /// <typeparam name="TCategoryName">The type whose name is used for the logger category name.</typeparam>
    internal class SerilogTelemetryLogger<TCategoryName> : ITelemetryLogger<TCategoryName>
    {
        private readonly ILogger<TCategoryName> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogTelemetryLogger{TCategoryName}"/> class.
        /// </summary>
#pragma warning disable S6672 // The Serilog telemetry logger should not implement ILogger<TCategoryName> directly, but allow consumers to specify the category name.
        public SerilogTelemetryLogger(ILogger<TCategoryName> logger)
#pragma warning restore S6672
        {
            _logger = logger ?? NullLogger<TCategoryName>.Instance;
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="string" /> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">Level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <returns>An <see cref="IDisposable" /> that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        /// <summary>
        /// Tracks a custom metric with contextual information to the configured observability backend.
        /// </summary>
        /// <typeparam name="TValue">The numerical unit of the metric value.</typeparam>
        /// <param name="metricName">The name of the custom metric.</param>
        /// <param name="metricValue">The current value of the custom metric.</param>
        /// <param name="telemetryContext">The additional information to provide context to the metric.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="metricName"/> is blank.</exception>
        public void LogCustomMetric<TValue>(string metricName, TValue metricValue, IDictionary<string, object> telemetryContext) where TValue : struct
        {
            var value = metricValue is double doubleValue ? doubleValue : Convert.ToDouble(metricValue);

            _logger.LogWarning(MessageFormats.MetricFormat, new MetricLogEntry(metricName, value, DateTimeOffset.UtcNow, new Dictionary<string, object>(telemetryContext)));
        }

        /// <summary>
        /// Tracks a custom event with contextual information to the configured observability backend.
        /// </summary>
        /// <param name="eventName">The name of the custom event.</param>
        /// <param name="timestamp">The time when the event occurred.</param>
        /// <param name="telemetryContext">The additional information to provide context to the event.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="eventName"/> is blank.</exception>
        public void LogCustomEvent(string eventName, DateTimeOffset timestamp, IDictionary<string, object> telemetryContext)
        {
            _logger.LogWarning(MessageFormats.EventFormat, new EventLogEntry(eventName, new Dictionary<string, object>(telemetryContext)));
        }

        /// <summary>
        /// Tracks a custom dependency with contextual information to the configured observability backend.
        /// </summary>
        /// <param name="dependencyName">The name of the custom dependency.</param>
        /// <param name="isSuccessful">The boolean flag to indicate whether the interaction with the dependency was successful.</param>
        /// <param name="startTime">The time when the dependency was contacted.</param>
        /// <param name="duration">The time it took for the dependency to respond or complete the request.</param>
        /// <param name="telemetryContext">The additional contextual information to provide context to the dependency.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyName"/> is blank.</exception>
        public void LogCustomDependency(string dependencyName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, IDictionary<string, object> telemetryContext)
        {
            (string dependencyType, object dependencyData, string targetName, string dependencyId) = telemetryContext switch
            {
                SerilogDependencyTelemetryContext context => (context.DependencyType, context.DependencyData, context.TargetName, context.DependencyId),
                _ => ("Unknown", null, null, null)
            };

            _logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType,
                dependencyName: dependencyName,
                dependencyData: dependencyData,
                targetName: targetName,
                duration: duration,
                startTime: startTime,
                dependencyId: dependencyId,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: new Dictionary<string, object>(telemetryContext)));
        }

        /// <summary>
        /// Tracks a custom request with contextual information to the configured observability backend.
        /// </summary>
        /// <remarks>
        ///     Make sure to dispose the returned operation result to flush the telemetry to the observability backend.
        /// </remarks>
        /// <param name="operationName">The name of the request operation.</param>
        /// <param name="telemetryContext">The additional contextual information to provide context to the request.</param>
        /// <returns>
        ///     The scoped request operation that allows to specify whether the request operation was successful.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="operationName"/> is blank.</exception>
        public RequestOperationResult LogCustomRequest(string operationName, IDictionary<string, object> telemetryContext)
        {
            string requestSource = telemetryContext is SerilogRequestTelemetryContext requestTelemetryContext
                ? requestTelemetryContext.RequestSource
                : "custom";

            return new SerilogRequestOperationResult((isSuccessful, startTime, duration) =>
            {
                var entry = RequestLogEntry.CreateForCustomRequest(
                    requestSource,
                    operationName,
                    isSuccessful,
                    duration,
                    startTime, new Dictionary<string, object>(telemetryContext));

                _logger.LogWarning(MessageFormats.RequestFormat, entry);
            });
        }

        private sealed class SerilogRequestOperationResult : RequestOperationResult
        {
            private readonly Action<bool, DateTimeOffset, TimeSpan> _stopOperation;

            internal SerilogRequestOperationResult(Action<bool, DateTimeOffset, TimeSpan> stopOperation)
            {
                _stopOperation = stopOperation;
            }

            protected override void StopOperation(bool isSuccessful, DateTimeOffset startTime, TimeSpan duration)
            {
                _stopOperation(isSuccessful, startTime, duration);
            }
        }
    }

    /// <summary>
    /// Represents the publicly available message formats to track telemetry.
    /// </summary>
    internal static class MessageFormats
    {
        /// <summary>
        /// Gets the message format to log external dependencies; compatible with Application Insights 'Dependencies'.
        /// </summary>
        public const string DependencyFormat = "{@" + ContextProperties.DependencyTracking.DependencyLogEntry + "}";

        /// <summary>
        /// Gets the message format to log events; compatible with Application Insights 'Events'.
        /// </summary>
        public const string EventFormat = "{@" + ContextProperties.EventTracking.EventLogEntry + "}";

        /// <summary>
        /// Gets the message format to log metrics; compatible with Application Insights 'Metrics'.
        /// </summary>
        public const string MetricFormat = "{@" + ContextProperties.MetricTracking.MetricLogEntry + "}";

        /// <summary>
        /// Gets the message format to log HTTP requests; compatible with Application Insights 'Requests'.
        /// </summary>
        public const string RequestFormat = "{@" + ContextProperties.RequestTracking.RequestLogEntry + "}";
    }

    internal static class ContextProperties
    {
        public const string TelemetryContext = "Context";

        public static class Correlation
        {
            public const string OperationId = "OperationId";
            public const string TransactionId = "TransactionId";
            public const string OperationParentId = "OperationParentId";
        }

        public static class DependencyTracking
        {
            public const string DependencyLogEntry = "Dependency";
            public const string DependencyId = "DependencyId";
            public const string DependencyType = "DependencyType";
            public const string TargetName = "DependencyTargetName";
            public const string DependencyName = "DependencyName";
            public const string DependencyData = "DependencyData";
            public const string StartTime = "DependencyStartTime";
            public const string ResultCode = "DependencyResultCode";
            public const string Duration = "DependencyDuration";
            public const string IsSuccessful = "DependencyIsSuccessful";
        }

        public static class EventTracking
        {
            public const string EventLogEntry = "Event";
            public const string EventName = "EventName";
        }

        public static class General
        {
            public const string ComponentName = "ComponentName";
            public const string MachineName = "MachineName";
            public const string TelemetryType = "TelemetryType";
        }

        public static class Kubernetes
        {
            public const string Namespace = "Namespace";
            public const string NodeName = "NodeName";
            public const string PodName = "PodName";
        }

        public static class RequestTracking
        {
            public const string RequestLogEntry = "Request";

            public static class ServiceBus
            {
                public const string Endpoint = "ServiceBus-Endpoint";
                public const string EntityName = "ServiceBus-Entity";
            }

            public static class EventHubs
            {
                public const string Namespace = "EventHubs-Namespace";
                public const string Name = "EventHubs-Name";
            }
        }

        public static class MetricTracking
        {
            public const string MetricLogEntry = "Metric";
            public const string Timestamp = "Timestamp";
        }
    }

    /// <summary>
    /// <para>Represents the supported types of telemetry available to track during extensions on the <see cref="ILogger"/>.</para>
    /// <para>Also compatible (but not limited to) with Azure Application Insights (see: <see href="https://observability.arcus-azure.net/features/sinks/azure-application-insights" /> for more information).</para>
    /// </summary>
    internal enum TelemetryType
    {
        /// <summary>
        /// Specifies the type of logged telemetry as traces.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Specifies the type of logged telemetry as tracked dependencies.
        /// </summary>
        Dependency = 1,

        /// <summary>
        /// Specifies the type of logged telemetry as HTTP requests.
        /// </summary>
        Request = 2,

        /// <summary>
        /// Specifies the type of logged telemetry as custom events.
        /// </summary>
        Events = 4,

        /// <summary>
        /// Specifies the type of logged telemetry as metrics.
        /// </summary>
        Metrics = 8
    }

    /// <summary>
    /// Represents the all formatting used when logging telemetry instances.
    /// </summary>
    internal static class FormatSpecifiers
    {
        /// <summary>
        /// A format specifier for converting DateTimeOffset instances to a string representation
        /// using the highest precision that is available.
        /// </summary>
        public static string InvariantTimestampFormat { get; } = "yyyy-MM-ddTHH:mm:ss.fffffff zzz";
    }

    /// <summary>
    /// Represents a telemetry context that can be used to enrich log entries with additional information.
    /// </summary>
    public class SerilogRequestTelemetryContext : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogRequestTelemetryContext"/> class.
        /// </summary>
        public SerilogRequestTelemetryContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogRequestTelemetryContext"/> class.
        /// </summary>
        /// <param name="userContext">The user context that will be copied to this telemetry context.</param>
        public SerilogRequestTelemetryContext(IDictionary<string, object> userContext) : base(userContext)
        {
        }

        /// <summary>
        /// Gets or sets the source of the request telemetry, which can be used to identify the caller (e.g., entity name of Azure Service Bus).
        /// </summary>
        public string RequestSource { get; set; }
    }

    /// <summary>
    /// Represents a telemetry context that can be used to enrich dependency log entries with additional information.
    /// </summary>
    public class SerilogDependencyTelemetryContext : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogDependencyTelemetryContext"/> class.
        /// </summary>
        public SerilogDependencyTelemetryContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogDependencyTelemetryContext"/> class.
        /// </summary>
        /// <param name="userContext">The user context that will be copied to this telemetry context.</param>
        public SerilogDependencyTelemetryContext(IDictionary<string, object> userContext) : base(userContext)
        {
        }

        /// <summary>
        /// Gets or sets the type of the dependency, which can be used to categorize the dependency (e.g., "SQL", "HTTP", etc.).
        /// </summary>
        public string DependencyType { get; set; }

        /// <summary>
        /// Gets or sets the data related to the dependency, which can be used to provide additional insights (e.g., SQL query, HTTP request details).
        /// </summary>
        public object DependencyData { get; set; }

        /// <summary>
        /// Gets or sets the target name of the dependency, which can be used to identify the specific instance of the dependency (e.g., database name, service URL).
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the dependency, which can be used to correlate multiple log entries related to the same dependency interaction.
        /// </summary>
        public string DependencyId { get; set; }
    }

    /// <summary>
    /// Represents a custom dependency as a logging entry.
    /// </summary>
    internal class DependencyLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyLogEntry"/> class.
        /// </summary>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of dependency target.</param>
        /// <param name="resultCode">The code of the result of the interaction with the dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="dependencyData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public DependencyLogEntry(
            string dependencyType,
            string dependencyName,
            object dependencyData,
            string targetName,
            string dependencyId,
            TimeSpan duration,
            DateTimeOffset startTime,
            int? resultCode,
            bool isSuccessful,
            IDictionary<string, object> context)
        {
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the dependency operation");

            DependencyId = dependencyId;
            DependencyType = dependencyType;
            DependencyName = dependencyName;
            DependencyData = dependencyData;
            TargetName = targetName;
            ResultCode = resultCode;
            IsSuccessful = isSuccessful;

            StartTime = startTime.ToString(FormatSpecifiers.InvariantTimestampFormat);
            Duration = duration;
            Context = context;
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Dependency;
        }

        /// <summary>
        /// Gets the ID of the dependency to link as parent ID.
        /// </summary>
        public string DependencyId { get; }

        /// <summary>
        /// Gets the custom type of the dependency.
        /// </summary>
        public string DependencyType { get; }

        /// <summary>
        /// Gets the name of the dependency.
        /// </summary>
        public string DependencyName { get; }

        /// <summary>
        /// Gets the custom data of the dependency.
        /// </summary>
        public object DependencyData { get; }

        /// <summary>
        /// Gets the name of the dependency target.
        /// </summary>
        public string TargetName { get; }

        /// <summary>
        /// Gets the code of the result of the interaction with the dependency.
        /// </summary>
        public int? ResultCode { get; }

        /// <summary>
        /// Gets the indication whether or not the operation was successful.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Gets the point in time when the interaction with the HTTP dependency was started.
        /// </summary>
        public string StartTime { get; }

        /// <summary>
        /// Gets the duration of the operation.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the context that provides more insights on the dependency that was measured.
        /// </summary>
        public IDictionary<string, object> Context { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string contextFormatted = $"{{{String.Join("; ", Context.Select(item => $"[{item.Key}, {item.Value}]"))}}}";
            return $"{DependencyType} {DependencyName} {DependencyData} named {TargetName} with ID {DependencyId} in {Duration} at {StartTime} " +
                   $"(IsSuccessful: {IsSuccessful} - ResultCode: {ResultCode} - Context: {contextFormatted})";
        }
    }

    /// <summary>
    /// Represents an event as a log entry.
    /// </summary>
    internal class EventLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogEntry" /> class.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="context">The context that provides more insights on the event that occurred.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is blank.</exception>
        public EventLogEntry(string name, IDictionary<string, object> context)
        {
            Guard.NotNullOrWhitespace(name, nameof(name), "Requires a non-blank event name to track an custom event");

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

    /// <summary>
    /// Represents a metric as a log entry.
    /// </summary>
    internal class MetricLogEntry
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
            Guard.NotNullOrWhitespace(name, nameof(name), "Requires a non-blank name to track a metric");

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

    /// <summary>
    /// Represents a HTTP request as a log entry.
    /// </summary>
    internal class RequestLogEntry
    {
        private RequestLogEntry(
            string method,
            string host,
            string uri,
            string operationName,
            int statusCode,
            string customRequestSource,
            string requestTime,
            TimeSpan duration,
            IDictionary<string, object> context)
            : this(method, host, uri, operationName, statusCode, RequestSourceSystem.Custom, requestTime, duration, context)
        {
            CustomRequestSource = customRequestSource;
        }

        private RequestLogEntry(
            string method,
            string host,
            string uri,
            string operationName,
            int statusCode,
            RequestSourceSystem sourceSystem,
            string requestTime,
            TimeSpan duration,
            IDictionary<string, object> context)
        {
            Guard.For<ArgumentException>(() => host?.Contains(" ") is true, "Requires a HTTP request host name without whitespace");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank operation name");
            Guard.NotLessThan(statusCode, 100, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotGreaterThan(statusCode, 599, nameof(statusCode), "Requires a HTTP response status code that's within the 100-599 range to track a HTTP request");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request operation");

            RequestMethod = method;
            RequestHost = host;
            RequestUri = uri;
            ResponseStatusCode = statusCode;
            RequestDuration = duration;
            OperationName = operationName;
            SourceSystem = sourceSystem;
            RequestTime = requestTime;
            Context = context;
            Context[ContextProperties.General.TelemetryType] = TelemetryType.Request;
        }

        /// <summary>
        /// Creates an <see cref="RequestLogEntry"/> instance for custom requests.
        /// </summary>
        /// <param name="requestSource">The source for the request telemetry to identifying the caller (ex. entity name of Azure Service Bus).</param>
        /// <param name="operationName">The name of the operation of the request.</param>
        /// <param name="isSuccessful">The indication whether or not the custom request was successfully processed.</param>
        /// <param name="duration">The duration it took to process the custom request.</param>
        /// <param name="startTime">The time when the request was received.</param>
        /// <param name="context">The telemetry context that provides more insights on the custom request.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static RequestLogEntry CreateForCustomRequest(
            string requestSource,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            Guard.NotNullOrWhitespace(requestSource, nameof(requestSource), "Requires a non-blank request source to identify the caller");
            Guard.NotNullOrWhitespace(operationName, nameof(operationName), "Requires a non-blank operation name");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the request duration");

            return CreateWithoutHttpRequest(requestSource, operationName, isSuccessful, duration, startTime, context);
        }

        private static RequestLogEntry CreateWithoutHttpRequest(
            string requestSource,
            string operationName,
            bool isSuccessful,
            TimeSpan duration,
            DateTimeOffset startTime,
            IDictionary<string, object> context)
        {
            return new RequestLogEntry(
                method: "<not-applicable>",
                host: "<not-applicable>",
                uri: "<not-applicable>",
                operationName: operationName,
                statusCode: isSuccessful ? 200 : 500,
                customRequestSource: requestSource,
                requestTime: startTime.ToString(FormatSpecifiers.InvariantTimestampFormat),
                duration: duration,
                context: context);
        }

        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        public string RequestMethod { get; }

        /// <summary>
        /// Gets the host that was requested.
        /// </summary>
        public string RequestHost { get; }

        /// <summary>
        /// Gets ths URI of the request.
        /// </summary>
        public string RequestUri { get; }

        /// <summary>
        /// Gets the HTTP response status code that was returned by the service.
        /// </summary>
        public int ResponseStatusCode { get; }

        /// <summary>
        /// Gets the duration of the processing of the request.
        /// </summary>
        public TimeSpan RequestDuration { get; }

        /// <summary>
        /// Gets the date when the request occurred.
        /// </summary>
        public string RequestTime { get; }

        /// <summary>
        /// Gets the type of source system from where the request came from.
        /// </summary>
        public RequestSourceSystem SourceSystem { get; set; }

        /// <summary>
        /// Gets the custom request source if the <see cref="SourceSystem"/> is <see cref="RequestSourceSystem.Custom"/>.
        /// </summary>
        public string CustomRequestSource { get; }

        /// <summary>
        /// Gets the name of the operation of the source system from where the request came from.
        /// </summary>
        public string OperationName { get; }

        /// <summary>
        /// Gets the context that provides more insights on the HTTP request that was tracked.
        /// </summary>
        public IDictionary<string, object> Context { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var contextFormatted = $"{{{String.Join("; ", Context.Select(item => $"[{item.Key}, {item.Value}]"))}}}";

            if (SourceSystem is RequestSourceSystem.Http)
            {
                return $"{RequestMethod} {RequestHost}/{RequestUri} from {OperationName} completed with {ResponseStatusCode} in {RequestDuration} at {RequestTime} - (Context: {contextFormatted})";
            }

            string source = DetermineSource();
            bool isSuccessful = ResponseStatusCode is 200;

            return $"{source} from {OperationName} completed in {RequestDuration} at {RequestTime} - (IsSuccessful: {isSuccessful}, Context: {contextFormatted})";
        }

        private string DetermineSource()
        {
            switch (SourceSystem)
            {
                case RequestSourceSystem.AzureServiceBus: return "Azure Service Bus";
                case RequestSourceSystem.AzureEventHubs: return "Azure EventHubs";
                case RequestSourceSystem.Custom: return "Custom " + CustomRequestSource;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SourceSystem), "Cannot determine request source as it represents something outside the bounds of the enumeration");
            }
        }
    }

    /// <summary>
    /// Represents the system from where the request came from.
    /// </summary>
    internal enum RequestSourceSystem
    {
        /// <summary>
        /// Specifies that the request-source is an Azure Service Bus queue or topic.
        /// </summary>
        AzureServiceBus = 1,

        /// <summary>
        /// Specifies that the request-source is a HTTP request
        /// </summary>
        Http = 2,

        /// <summary>
        /// Specifies that the request-source is an Azure EventHubs.
        /// </summary>
        AzureEventHubs = 4,

        /// <summary>
        /// Specifies that the request-source is a custom system.
        /// </summary>
        Custom = 8
    }
}
