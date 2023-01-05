using System;
using System.Linq;
using GuardNet;

namespace Arcus.Observability.Telemetry.Core.Iot
{
    /// <summary>
    /// Represents the instance to parse the IoT Hub connection string to a strongly-typed <see cref="IotHubConnectionStringParserResult"/>.
    /// </summary>
    internal static class IotHubConnectionStringParser
    {
        /// <summary>
        /// Parses the incoming IoT Hub <paramref name="connectionString"/> to a strongly-typed result of IoT Hub properties.
        /// </summary>
        /// <param name="connectionString">The connection string based on the hostname of the IoT Hub service.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="connectionString"/> is blank.</exception>
        internal static IotHubConnectionStringParserResult Parse(string connectionString)
        {
            Guard.NotNullOrWhitespace(connectionString, nameof(connectionString), "Requires a non-blank connection string based on the hostname of the IoT Hub service");

            string hostNameProperty =
                connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault(part => part.StartsWith("HostName", StringComparison.OrdinalIgnoreCase));

            if (hostNameProperty is null)
            {
                throw new FormatException(
                    "Cannot parse IoT Hub connection string because cannot find 'HostName' in IoT Hub connection string");
            }

            string hostName = 
                string.Join("", hostNameProperty.SkipWhile(ch => ch != '=').Skip(1));

            return new IotHubConnectionStringParserResult(hostName);
        }
    }
}
