﻿using System;
using GuardNet;

namespace Arcus.Observability.Telemetry.Core.Iot
{
    /// <summary>
    /// Represents the result of the <see cref="IotHubConnectionStringParser"/> when parsing an IoT Hub connection string.
    /// </summary>
    internal class IotHubConnectionStringParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IotHubConnectionStringParserResult" /> class.
        /// </summary>
        /// <param name="hostName">The fully-qualified DNS hostname of the IoT Hub service.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="hostName"/> is blank.</exception>
        internal IotHubConnectionStringParserResult(string hostName)
        {
            Guard.NotNullOrWhitespace(hostName, nameof(hostName), "Requires a non-blank fully-qualified DNS hostname of the IoT Hub service");
            HostName = hostName;
        }

        /// <summary>
        /// Gets the value of the fully-qualified DNS hostname of the IoT Hub service.
        /// </summary>
        internal string HostName { get; }
    }
}