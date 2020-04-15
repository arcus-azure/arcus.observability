using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    public static class ILoggerExtensions
    {
        private const string RequestFormat =
            MessagePrefixes.RequestViaHttp + " {"
            + ContextProperties.RequestTracking.RequestMethod + "} {"
            + ContextProperties.RequestTracking.RequestHost + "}/{" 
            + ContextProperties.RequestTracking.RequestUri + "} completed with {"
            + ContextProperties.RequestTracking.ResponseStatusCode + "} in {"
            + ContextProperties.RequestTracking.RequestDuration + "} at {"
            + ContextProperties.RequestTracking.RequestTime + "} - (Context: {@"
            + ContextProperties.EventTracking.EventContext + "})";

        private const string DependencyFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} {"
            + ContextProperties.DependencyTracking.DependencyData + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.EventTracking.EventContext + "})";

        private const string ServiceBusDependencyFormat =
            MessagePrefixes.Dependency + " Azure Service Bus {"
            + ContextProperties.DependencyTracking.ServiceBus.EntityType + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} {"
            + ContextProperties.DependencyTracking.DependencyType + "} in {" 
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.EventTracking.EventContext + "})";

        private const string HttpDependencyFormat =
            MessagePrefixes.DependencyViaHttp + " {"
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName + "} completed with {"
            + ContextProperties.DependencyTracking.ResultCode + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.EventTracking.EventContext + "})";

        private const string SqlDependencyFormat =
            MessagePrefixes.DependencyViaSql + " {" 
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName
            + "} for operation {" + ContextProperties.DependencyTracking.DependencyData
            + "} in {" + ContextProperties.DependencyTracking.Duration
            + "} at {" + ContextProperties.DependencyTracking.StartTime
            + "} (Successful: {" + ContextProperties.DependencyTracking.IsSuccessful
            + "} - Context: {@" + ContextProperties.EventTracking.EventContext + "})";

        private const string EventFormat = 
            MessagePrefixes.Event + " {" 
            + ContextProperties.EventTracking.EventName 
            + "} (Context: {@" + ContextProperties.EventTracking.EventContext + "})";

        private const string MetricFormat =
            MessagePrefixes.Metric + " {" 
            + ContextProperties.MetricTracking.MetricName + "}: {" 
            + ContextProperties.MetricTracking.MetricValue 
            + "} (Context: {@" + ContextProperties.EventTracking.EventContext + "})";

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        public static void LogRequest(this ILogger logger, HttpRequest request, HttpResponse response, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(response, nameof(response));
            Guard.For<ArgumentException>(() => request.Scheme != null && request.Scheme.Contains(" "), "HTTP request scheme cannot contain whitespace");
            Guard.For<ArgumentException>(() => !request.Host.HasValue, "HTTP request host requires a value");
            Guard.For<ArgumentException>(() => request.Host.ToString()?.Contains(" ") == true, "HTTP request host name cannot contain whitespace");

            context = context ?? new Dictionary<string, object>();

            int statusCode = response.StatusCode;
            PathString resourcePath = request.Path;
            string host = $"{request.Scheme}://{request.Host}";

            logger.LogInformation(RequestFormat, request.Method, host, resourcePath, statusCode, duration, DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture), context);
        }

        /// <summary>
        ///     Logs a dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogDependency(this ILogger logger, string dependencyType, object dependencyData, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType));
            Guard.NotNull(dependencyData, nameof(dependencyData));

            LogDependency(logger, dependencyType, dependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs a dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogDependency(this ILogger logger, string dependencyType, object dependencyData, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType));
            Guard.NotNull(dependencyData, nameof(dependencyData));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyFormat, dependencyType, dependencyData, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="queueName">Name of the Service Bus queue</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusQueueDependency(this ILogger logger, string queueName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(measurement, nameof(measurement));

            LogServiceBusQueueDependency(logger, queueName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="queueName">Name of the Service Bus queue</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusQueueDependency(this ILogger logger, string queueName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            
            LogServiceBusDependency(logger, queueName, isSuccessful, startTime, duration, ServiceBusEntityType.Queue, context);
        }

        /// <summary>
        ///     Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="topicName">Name of the Service Bus topic</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusTopicDependency(this ILogger logger, string topicName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(measurement, nameof(measurement));

            LogServiceBusTopicDependency(logger, topicName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="topicName">Name of the Service Bus topic</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusTopicDependency(this ILogger logger, string topicName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            
            LogServiceBusDependency(logger, topicName, isSuccessful, startTime, duration, ServiceBusEntityType.Topic, context);
        }

        /// <summary>
        ///     Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="entityName">Name of the Service Bus entity</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="entityType">Type of the Service Bus entity</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusDependency(this ILogger logger, string entityName, bool isSuccessful, DependencyMeasurement measurement, ServiceBusEntityType entityType = ServiceBusEntityType.Unknown, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(measurement, nameof(measurement));

            LogServiceBusDependency(logger, entityName, isSuccessful, measurement.StartTime, measurement.Elapsed, entityType, context);
        }

        /// <summary>
        ///     Logs an Azure Service Bus Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="entityName">Name of the Service Bus entity</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="entityType">Type of the Service Bus entity</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusDependency(this ILogger logger, string entityName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, ServiceBusEntityType entityType = ServiceBusEntityType.Unknown, Dictionary<string, object> context = null)
        {
            logger.LogInformation(ServiceBusDependencyFormat, entityType, entityName, "Azure Resource", duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that started the HTTP communication</param>
        /// <param name="statusCode">Status code that was returned by the service for this HTTP communication</param>
        /// <param name="measurement">Measuring the latency of the HTTP dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogHttpDependency(this ILogger logger, HttpRequestMessage request, HttpStatusCode statusCode, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(measurement, nameof(measurement));

            LogHttpDependency(logger, request, statusCode, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an HTTP dependency
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that started the HTTP communication</param>
        /// <param name="statusCode">Status code that was returned by the service for this HTTP communication</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogHttpDependency(this ILogger logger, HttpRequestMessage request, HttpStatusCode statusCode, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));

            context = context ?? new Dictionary<string, object>();

            Uri requestUri = request.RequestUri;
            string targetName = requestUri.Host;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            bool isSuccessful = (int) statusCode >= 200 && (int) statusCode < 300;

            logger.LogInformation(HttpDependencyFormat, targetName, dependencyName, (int) statusCode, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs a SQL dependency
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="serverName">Name of server hosting the database</param>
        /// <param name="databaseName">Name of database</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the SQL dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogSqlDependency(this ILogger logger, string serverName, string databaseName, string tableName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(serverName, nameof(serverName));
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName));
            Guard.NotNullOrWhitespace(tableName, nameof(tableName));
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, serverName, databaseName, tableName, measurement.DependencyData, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs a SQL dependency
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="serverName">Name of server hosting the database</param>
        /// <param name="databaseName">Name of database</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="operationName">Name of the operation that was performed</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogSqlDependency(this ILogger logger, string serverName, string databaseName, string tableName, string operationName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(serverName, nameof(serverName));
            Guard.NotNullOrWhitespace(databaseName, nameof(databaseName));
            Guard.NotNullOrWhitespace(tableName, nameof(tableName));
            Guard.NotNullOrWhitespace(operationName, nameof(operationName));

            context = context ?? new Dictionary<string, object>();

            string dependencyName = $"{databaseName}/{tableName}";

            logger.LogInformation(SqlDependencyFormat, serverName, dependencyName, operationName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs a custom event
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="name">Name of the event</param>
        /// <param name="context">Context that provides more insights on the event that occured</param>
        public static void LogEvent(this ILogger logger, string name, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(name, nameof(name));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(EventFormat, name, context);
        }

        /// <summary>
        ///     Logs a custom metric
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="context">Context that provides more insights on the event that occured</param>
        public static void LogMetric(this ILogger logger, string name, double value, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(name, nameof(name));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(MetricFormat, name, value, context);
        }

        /// <summary>
        ///     Logs an event related to an security activity (i.e. input validation, authentication, authorization...).
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="name">The user message written.</param>
        /// <param name="context">The context that provides more insights on the event that occured.</param>
        public static void LogSecurityEvent(this ILogger logger, string name, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(name, nameof(name));

            context = context ?? new Dictionary<string, object>();
            context["EventType"] = "Security";

            LogEvent(logger, name, context);
        }
    }
}