using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Microsoft.Azure.Devices.Client;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions on the <see cref="ILogger"/> related to tracking IoT dependencies.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        /// <summary>
        ///     Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="iotHubConnectionString">Name of the IoT Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogIotHubDependency(this ILogger logger, string iotHubConnectionString, bool isSuccessful, DependencyMeasurement measurement, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString));

            LogIotHubDependency(logger, iotHubConnectionString, isSuccessful, measurement.StartTime, measurement.Elapsed, context);
        }

        /// <summary>
        ///     Logs an Azure Iot Hub Dependency.
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="iotHubConnectionString">Name of the Event Hub resource</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        public static void LogIotHubDependency(this ILogger logger, string iotHubConnectionString, bool isSuccessful, DateTimeOffset startTime, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhitespace(iotHubConnectionString, nameof(iotHubConnectionString));

            context = context ?? new Dictionary<string, object>();

            var iotHubConnection = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            logger.LogIotHubDependency(iotHubName: iotHubConnection.HostName, isSuccessful: isSuccessful, startTime: startTime, duration: duration, context: context);
        }
    }
}
