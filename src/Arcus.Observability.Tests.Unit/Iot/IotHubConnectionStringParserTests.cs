using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Iot;
using Bogus;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Unit.Iot
{
    public class IotHubConnectionStringParserTests
    {
        private readonly ITestOutputHelper _outputWriter;
        private static readonly Faker Bogus = new Faker();

        [Flags]
        private enum ExcludeIotHubProperty { None = 0, HostName = 1, DeviceId = 2, SharedAccessKey = 4, SharedAccessKeyName = 8 }

        /// <summary>
        /// Initializes a new instance of the <see cref="IotHubConnectionStringParserTests" /> class.
        /// </summary>
        public IotHubConnectionStringParserTests(ITestOutputHelper outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public static IEnumerable<object[]> ConnectionString =>
            Enumerable.Range(1, 100)
                      .Select(i => new object[] { RandomConnectionString() });

        [Theory]
        [MemberData(nameof(ConnectionString))]
        public void ParseOriginal_WithRandomIotHubConnectionString_Succeeds(string connectionString)
        {
            _outputWriter.WriteLine("ConnectionString: {0}", connectionString);
            var builder = IotHubConnectionStringBuilder.Create(connectionString);

            Assert.NotNull(builder.HostName);
        }

        [Theory]
        [MemberData(nameof(ConnectionString))]
        public void Parse_WithIotHubConnectionString_Succeeds(string connectionString)
        {
            // Arrange
            _outputWriter.WriteLine("ConnectionString: {0}", connectionString);
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            DateTimeOffset startTime = Bogus.Date.RecentOffset();
            TimeSpan duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();

            // Act
            logger.LogIotHubDependencyWithConnectionString(connectionString, isSuccessful, startTime, duration, dependencyId);

            // Assert
            var builder = IotHubConnectionStringBuilder.Create(connectionString);
            Assert.StartsWith($"Azure IoT Hub {builder.HostName}", logger.WrittenMessage);
        }

        [Theory]
        [MemberData(nameof(ConnectionString))]
        public void ParseWithDurationMeasurement_WithIotHubConnectionString_Succeeds(string connectionString)
        {
            // Arrange
            _outputWriter.WriteLine("ConnectionString: {0}", connectionString);
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            var dependencyId = Bogus.Random.Guid().ToString();

            // Act
            logger.LogIotHubDependencyWithConnectionString(connectionString, isSuccessful, measurement, dependencyId);

            // Assert
            var builder = IotHubConnectionStringBuilder.Create(connectionString);
            Assert.StartsWith($"Azure IoT Hub {builder.HostName}", logger.WrittenMessage);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public static void Parse_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            DateTimeOffset startTime = Bogus.Date.RecentOffset();
            TimeSpan duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();

            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependencyWithConnectionString(connectionString, isSuccessful, startTime, duration, dependencyId));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public static void ParseWithDurationMeasurement_WithoutConnectionString_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            var dependencyId = Bogus.Random.Guid().ToString();

            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogIotHubDependencyWithConnectionString(connectionString, isSuccessful, measurement, dependencyId));
        }

        public static IEnumerable<object[]> ConnectionStringWithoutHostName =>
            Enumerable.Range(1, 100)
                      .Select(i => new object[] { RandomConnectionString(ExcludeIotHubProperty.HostName) });

        [Theory]
        [MemberData(nameof(ConnectionStringWithoutHostName))]
        public void Parse_WithoutHostName_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            DateTimeOffset startTime = Bogus.Date.RecentOffset();
            TimeSpan duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();

            // Act / Assert
            Assert.ThrowsAny<FormatException>(
                () => logger.LogIotHubDependencyWithConnectionString(connectionString, isSuccessful, startTime, duration, dependencyId));
        }

        [Theory]
        [MemberData(nameof(ConnectionStringWithoutHostName))]
        public void ParseWithDurationMeasurement_WithoutHostName_Fails(string connectionString)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = Bogus.Random.Bool();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            var dependencyId = Bogus.Random.Guid().ToString();

            // Act / Assert
            Assert.ThrowsAny<FormatException>(
                () => logger.LogIotHubDependencyWithConnectionString(connectionString, isSuccessful, measurement, dependencyId));
        }

        private static string RandomConnectionString(ExcludeIotHubProperty exclude = ExcludeIotHubProperty.None)
        {
            string[] alphanumericInputs =
                Enumerable.Range('a', 26)
                          .Concat(Enumerable.Range('A', 26))
                          .Concat(Enumerable.Range('0', 10))
                          .Select(char.ConvertFromUtf32)
                          .ToArray();

            string hostName = PickRandom(alphanumericInputs.Concat(new[] { "_", "-" }), 100);
            string iotHubName = PickRandom(alphanumericInputs.Concat(new[] { "_", "-" }), 100);

            string hostNameProperty = CreateProperty("HostName", $"{iotHubName}.{hostName}");
            string deviceIdProperty = CreateProperty("DeviceId", 
                PickRandom(alphanumericInputs.Concat(new[] { "-", ":", ".", "+", "%", "_", "#", "*", "?", "!", "(", ")", ",", "=", "@", "$", "'" }), 128));
            
            string sharedAccessKey =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        Bogus.Random.String(length: Bogus.Random.Int(1, 100))));
            string sharedAccessKeyProperty = CreateProperty("SharedAccessKey", sharedAccessKey);
            string sharedAccessKeyNameProperty = CreateOptionalProperty("SharedAccessKeyName", PickRandom(alphanumericInputs.Concat(new[] { ".", "_", "-", "@" }), 100));

            return CombineProperties(
                exclude.HasFlag(ExcludeIotHubProperty.HostName) ? null : hostNameProperty,
                exclude.HasFlag(ExcludeIotHubProperty.DeviceId) ? null : deviceIdProperty,
                exclude.HasFlag(ExcludeIotHubProperty.SharedAccessKeyName) ? null : sharedAccessKeyNameProperty,
                exclude.HasFlag(ExcludeIotHubProperty.SharedAccessKey) ? null : sharedAccessKeyProperty);
        }

        private static string PickRandom(IEnumerable<string> inputs, int maxLength)
        {
            inputs = inputs.ToArray();
            return string.Join("",
                Enumerable.Range(1, Bogus.Random.Int(1, maxLength))
                          .Select(i => Bogus.PickRandom(inputs)));
        }

        private static string CreateProperty(string key, object value)
        {
            return RandomCase(key) + "=" + value;
        }

        private static string CreateOptionalProperty(string key, object value)
        {
            string name = RandomCase(key).OrNull(Bogus);
            if (name is null)
            {
                return null;
            }

            return name + "=" + value;
        }

        private static string RandomCase(string item)
        {
            return string.Join("", item.ToCharArray().Select(ch =>
            {
                if (Bogus.Random.Bool())
                {
                    return char.ToUpper(ch);
                }

                return char.ToLower(ch);
            }));
        }

        private static string CombineProperties(params string[] properties)
        {
            return string.Join(";", Bogus.Random.Shuffle(properties.Where(prop => prop != null)));
        }
    }
}
