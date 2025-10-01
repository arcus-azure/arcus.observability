using System;

namespace Arcus.Observability.Telemetry.Core.Logging
{
    /// <summary>
    /// Represents the system from where the request came from.
    /// </summary>
    [Obsolete("Will be moved to Serilog-specific project in v4.0")]
    public enum RequestSourceSystem
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