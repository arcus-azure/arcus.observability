using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

            context = context ?? new Dictionary<string, object>();

            var statusCode = response.StatusCode;
            var resourcePath = request.Path;
            var host = $"{request.Scheme}://{request.Host}";

            logger.LogInformation(RequestFormat, request.Method, host, resourcePath, statusCode, duration, DateTimeOffset.UtcNow, context);
        }

        private const string HttpDependencyFormat =
            MessagePrefixes.DependencyViaHttp + "{"
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName + "} completed with {"
            + ContextProperties.DependencyTracking.ResultCode + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: "
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.EventTracking.EventContext + "})";

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
            bool isSuccessful = (int) statusCode >= 200 && (int)statusCode < 300;

            logger.LogInformation(HttpDependencyFormat, targetName, dependencyName, (int) statusCode, duration, startTime, isSuccessful, context);
        }

        private const string SqlDependencyFormat =
            MessagePrefixes.DependencyViaSql + " {" 
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName
            + "} for operation {" + ContextProperties.DependencyTracking.DependencyData
            + "} in {" + ContextProperties.DependencyTracking.Duration
            + "} at {" + ContextProperties.DependencyTracking.StartTime
            + "} (Successful: {" + ContextProperties.DependencyTracking.IsSuccessful
            + "} - Context: {@" + ContextProperties.EventTracking.EventContext + "})";

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

            var dependencyName = $"{databaseName}/{tableName}";

            logger.LogInformation(SqlDependencyFormat, serverName, dependencyName, operationName, duration, startTime, isSuccessful, context);
        }

        private const string EventFormat = 
            MessagePrefixes.Event + " {" 
            + ContextProperties.EventTracking.EventName 
            + "} (Context: {@" + ContextProperties.EventTracking.EventContext + "})";

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

        private const string MetricFormat =
            MessagePrefixes.Metric + " {" 
            + ContextProperties.MetricTracking.MetricName + "}: {" 
            + ContextProperties.MetricTracking.MetricValue 
            + "} (Context: {@" + ContextProperties.EventTracking.EventContext + "})";

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
        /// <param name="categoryName">The name of the security category.</param>
        /// <param name="message">The user message written with in the <paramref name="categoryName"/>.</param>
        /// <param name="arguments">The arguments formatted with the <paramref name="message"/>.</param>
        public static void LogSecurityEvent(this ILogger logger, string categoryName, string message, params object[] arguments)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(categoryName, nameof(categoryName), "Security category name cannot be blank");

            LogSecurityEvent(logger, LogLevel.Warning, categoryName, message, arguments);
        }

        private const string SecurityPrefix = "{" + MessagePrefixes.SecurityEvent + "} ";

        /// <summary>
        ///     Logs an event related to an security activity (i.e. input validation, authentication, authorization...).
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="level">The level at which the security event should be written.</param>
        /// <param name="categoryName">The name of the security category.</param>
        /// <param name="message">The user message written with in the <paramref name="categoryName"/>.</param>
        /// <param name="arguments">The arguments formatted with the <paramref name="message"/>.</param>
        public static void LogSecurityEvent(this ILogger logger, LogLevel level, string categoryName, string message, params object[] arguments)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(categoryName, nameof(categoryName), "Security category name cannot be blank");
            
            arguments = arguments ?? new object[0];

            logger.Log(level, SecurityPrefix + message, arguments.Prepend(categoryName));
        }
    }
}