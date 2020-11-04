using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            + ContextProperties.TelemetryContext + "})";

        private const string DependencyFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} {"
            + ContextProperties.DependencyTracking.DependencyData + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        private const string DependencyWithoutDataFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        private const string ServiceBusDependencyFormat =
            MessagePrefixes.Dependency + " {"
            + ContextProperties.DependencyTracking.DependencyType + "} {"
            + ContextProperties.DependencyTracking.ServiceBus.EntityType + "} named {"
            + ContextProperties.DependencyTracking.TargetName + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        private const string HttpDependencyFormat =
            MessagePrefixes.DependencyViaHttp + " {"
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName + "} completed with {"
            + ContextProperties.DependencyTracking.ResultCode + "} in {"
            + ContextProperties.DependencyTracking.Duration + "} at {"
            + ContextProperties.DependencyTracking.StartTime + "} (Successful: {"
            + ContextProperties.DependencyTracking.IsSuccessful + "} - Context: {@"
            + ContextProperties.TelemetryContext + "})";

        private const string SqlDependencyFormat =
            MessagePrefixes.DependencyViaSql + " {" 
            + ContextProperties.DependencyTracking.TargetName + "} for {"
            + ContextProperties.DependencyTracking.DependencyName
            + "} for operation {" + ContextProperties.DependencyTracking.DependencyData
            + "} in {" + ContextProperties.DependencyTracking.Duration
            + "} at {" + ContextProperties.DependencyTracking.StartTime
            + "} (Successful: {" + ContextProperties.DependencyTracking.IsSuccessful
            + "} - Context: {@" + ContextProperties.TelemetryContext + "})";

        private const string EventFormat = 
            MessagePrefixes.Event + " {" 
            + ContextProperties.EventTracking.EventName
#pragma warning disable 618 // Use 'ContextProperties.TelemetryContext' once we remove 'EventDescription'.
            + "} (Context: {@" + ContextProperties.EventTracking.EventContext + "})";
#pragma warning restore 618

        private const string MetricFormat =
            MessagePrefixes.Metric + " {" 
            + ContextProperties.MetricTracking.MetricName + "}: {" 
            + ContextProperties.MetricTracking.MetricValue + "} at {"
            + ContextProperties.MetricTracking.Timestamp
            + "} (Context: {@" + ContextProperties.TelemetryContext + "})";

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


            LogRequest(logger, request, response.StatusCode, duration, context);
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        public static void LogRequest(this ILogger logger, HttpRequest request, int responseStatusCode, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.For<ArgumentException>(() => request.Scheme != null && request.Scheme.Contains(" "), "HTTP request scheme cannot contain whitespace");
            Guard.For<ArgumentException>(() => !request.Host.HasValue, "HTTP request host requires a value");
            Guard.For<ArgumentException>(() => request.Host.ToString()?.Contains(" ") == true, "HTTP request host name cannot contain whitespace");

            context = context ?? new Dictionary<string, object>();

            PathString resourcePath = request.Path;
            string host = $"{request.Scheme}://{request.Host}";

            logger.LogInformation(RequestFormat, request.Method, host, resourcePath, responseStatusCode, duration, DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture), context);
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        public static void LogRequest(this ILogger logger, HttpRequestMessage request, HttpResponseMessage response, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(response, nameof(response));
            Guard.NotNull(request.RequestUri, nameof(request.RequestUri));
            Guard.For<ArgumentException>(() => request.RequestUri.Scheme?.Contains(" ") == true, "HTTP request scheme cannot contain whitespace");
            Guard.For<ArgumentException>(() => request.RequestUri.Host?.Contains(" ") == true, "HTTP request host name cannot contain whitespace");
            
            LogRequest(logger, request, response.StatusCode, duration, context);
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        public static void LogRequest(this ILogger logger, HttpRequestMessage request, HttpStatusCode responseStatusCode, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(request.RequestUri, nameof(request.RequestUri));
            Guard.For<ArgumentException>(() => request.RequestUri.Scheme?.Contains(" ") == true, "HTTP request scheme cannot contain whitespace");
            Guard.For<ArgumentException>(() => request.RequestUri.Host?.Contains(" ") == true, "HTTP request host name cannot contain whitespace");

            context = context ?? new Dictionary<string, object>();

            var statusCode = (int)responseStatusCode;
            PathString resourcePath = request.RequestUri.AbsolutePath;
            string host = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}";

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

            LogDependency(logger, dependencyType, dependencyData, targetName: null, isSuccessful: isSuccessful, startTime: startTime, duration: duration, context: context);
        }

        /// <summary>
        ///     Logs a dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="targetName">Name of the dependency target</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Service Bus dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogDependency(this ILogger logger, string dependencyType, object dependencyData, string targetName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType));
            Guard.NotNull(dependencyData, nameof(dependencyData));

            LogDependency(logger, dependencyType, dependencyData, targetName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs a dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="dependencyType">Custom type of dependency</param>
        /// <param name="dependencyData">Custom data of dependency</param>
        /// <param name="targetName">Name of dependency target</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogDependency(this ILogger logger, string dependencyType, object dependencyData, string targetName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType));
            Guard.NotNull(dependencyData, nameof(dependencyData));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyFormat, dependencyType, dependencyData, targetName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="searchServiceName">Name of the Azure Search service</param>
        /// <param name="operationName">Name of the operation to execute on the Azure Search service</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogAzureSearchDependency(this ILogger logger, string searchServiceName, string operationName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName));
            Guard.NotNullOrWhitespace(operationName, nameof(operationName));

            context = context ?? new Dictionary<string, object>();

            LogAzureSearchDependency(logger, searchServiceName, operationName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Search Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="searchServiceName">Name of the Azure Search service</param>
        /// <param name="operationName">Name of the operation to execute on the Azure Search service</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogAzureSearchDependency(this ILogger logger, string searchServiceName, string operationName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(searchServiceName, nameof(searchServiceName));
            Guard.NotNullOrWhitespace(operationName, nameof(operationName));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyFormat, "Azure Search", operationName, searchServiceName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
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
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="entityType">Type of the Service Bus entity</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogServiceBusDependency(this ILogger logger, string entityName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, ServiceBusEntityType entityType = ServiceBusEntityType.Unknown, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(entityName, nameof(entityName));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(ServiceBusDependencyFormat, "Azure Service Bus", entityType, entityName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="containerName">Name of the Blob Container resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogBlobStorageDependency(this ILogger logger, string accountName, string containerName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(accountName, nameof(accountName));
            Guard.NotNullOrWhitespace(containerName, nameof(containerName));
            Guard.NotNull(measurement, nameof(measurement));

            LogBlobStorageDependency(logger, accountName, containerName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Blob Storage Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="containerName">Name of the Blob Container resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogBlobStorageDependency(this ILogger logger, string accountName, string containerName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(accountName, nameof(accountName));
            Guard.NotNullOrWhitespace(containerName, nameof(containerName));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyFormat, "Azure blob", containerName, accountName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="tableName">Name of the Table Storage resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the Table Storage dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogTableStorageDependency(this ILogger logger, string accountName, string tableName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(tableName, nameof(tableName));
            Guard.NotNullOrWhitespace(accountName, nameof(accountName));
            Guard.NotNull(measurement, nameof(measurement));

            LogTableStorageDependency(logger, accountName: accountName, tableName: tableName, isSuccessful: isSuccessful, startTime: measurement.StartTime, duration: measurement.Elapsed, context: context);
        }

        /// <summary>
        ///     Logs an Azure Table Storage Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="accountName">Account of the storage resource</param>
        /// <param name="tableName">Name of the Table Storage resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogTableStorageDependency(this ILogger logger, string accountName, string tableName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(tableName, nameof(tableName));
            Guard.NotNullOrWhitespace(accountName, nameof(accountName));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyFormat, "Azure table", tableName, accountName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="namespaceName">Namespace of the resource</param>
        /// <param name="eventHubName">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogEventHubsDependency(this ILogger logger, string namespaceName, string eventHubName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName));
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName));

            LogEventHubsDependency(logger, namespaceName, eventHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Event Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="namespaceName">Namespace of the resource</param>
        /// <param name="eventHubName">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogEventHubsDependency(this ILogger logger, string namespaceName, string eventHubName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(namespaceName, nameof(namespaceName));
            Guard.NotNullOrWhitespace(eventHubName, nameof(eventHubName));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyFormat, "Azure Event Hubs", namespaceName, eventHubName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
        }

        /// <summary>
        ///     Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="iotHubName">Name of the IoT Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogIotHubDependency(this ILogger logger, string iotHubName, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName));

            LogIotHubDependency(logger, iotHubName, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="iotHubName">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogIotHubDependency(this ILogger logger, string iotHubName, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(iotHubName, nameof(iotHubName));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(DependencyWithoutDataFormat, "Azure IoT Hub", iotHubName, duration, startTime.ToString(CultureInfo.InvariantCulture), isSuccessful, context);
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
        ///     Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="accountName">Name of the storage resource</param>
        /// <param name="database">Name of the database</param>
        /// <param name="container">Name of the container</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency of the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogCosmosSqlDependency(this ILogger logger, string accountName, string database, string container, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(accountName, nameof(accountName));
            Guard.NotNullOrWhitespace(database, nameof(database));
            Guard.NotNullOrWhitespace(container, nameof(container));

            LogCosmosSqlDependency(logger, accountName, database, container, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs a Cosmos SQL dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="accountName">Name of the storage resource</param>
        /// <param name="database">Name of the database</param>
        /// <param name="container">Name of the container</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogCosmosSqlDependency(this ILogger logger, string accountName, string database, string container, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(accountName, nameof(accountName));
            Guard.NotNullOrWhitespace(database, nameof(database));
            Guard.NotNullOrWhitespace(container, nameof(container));

            context = context ?? new Dictionary<string, object>();
            string data = $"{database}/{container}";

            logger.LogInformation(DependencyFormat, "Azure DocumentDB", data, accountName, duration, startTime, isSuccessful, context);
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
        /// <param name="connectionString">SQL connection string</param>	
        /// <param name="tableName">Name of table</param>	
        /// <param name="operationName">Name of the operation that was performed</param>	
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>	
        /// <param name="measurement">Measuring the latency to call the SQL dependency</param>	
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        [Obsolete("Will be moved to separate package 'Arcus.Observability.Telemetry.Sql' in the future")]
        public static void LogSqlDependency(this ILogger logger, string connectionString, string tableName, string operationName, DependencyMeasurement measurement, bool isSuccessful, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(measurement, nameof(measurement));

            LogSqlDependency(logger, connectionString, tableName, operationName, measurement.StartTime, measurement.Elapsed, isSuccessful, context);
        }

        /// <summary>	
        ///     Logs a SQL dependency	
        /// </summary>	
        /// <param name="logger">Logger to use</param>	
        /// <param name="connectionString">SQL connection string</param>	
        /// <param name="tableName">Name of table</param>	
        /// <param name="operationName">Name of the operation that was performed</param>	
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>	
        /// <param name="duration">Duration of the operation</param>	
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>	
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        [Obsolete("Will be moved to separate package 'Arcus.Observability.Telemetry.Sql' in the future")]
        public static void LogSqlDependency(this ILogger logger, string connectionString, string tableName, string operationName, DateTimeOffset startTime, TimeSpan duration, bool isSuccessful, Dictionary<string, object> context = null)
        {
            Guard.NotNullOrEmpty(connectionString, nameof(connectionString));
            var connection = new SqlConnectionStringBuilder(connectionString);

            LogSqlDependency(logger, connection.DataSource, connection.InitialCatalog, tableName, operationName, isSuccessful, startTime, duration, context);
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

            LogMetric(logger, name, value, DateTimeOffset.UtcNow, context);
        }

        /// <summary>
        ///     Logs a custom metric
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value of the metric</param>
        /// <param name="timestamp">Timestamp of the metric</param>
        /// <param name="context">Context that provides more insights on the event that occured</param>
        public static void LogMetric(this ILogger logger, string name, double value, DateTimeOffset timestamp, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(name, nameof(name));

            context = context ?? new Dictionary<string, object>();

            logger.LogInformation(MetricFormat, name, value, timestamp.ToString(CultureInfo.InvariantCulture), context);
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
