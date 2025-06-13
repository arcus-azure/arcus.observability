using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Serialization;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Arcus.Observability.Tests.Core;
using Arcus.Observability.Tests.Unit.Fixture;
using Bogus;
using Bogus.DataSets;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit;
using static Arcus.Observability.Telemetry.Core.ContextProperties;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class ApplicationInsightsTelemetryConverterTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogDependency_WithDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string dependencyType = $"dependency-{Guid.NewGuid()}";
            const int dependencyData = 10;
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);

            Order order = OrderGenerator.Generate();
            string xml = SerializeXml(order);
            var telemetryContext = new Dictionary<string, object>
            {
                ["CustomSetting"] = "Approved",
                ["CustomXml"] = xml
            };
            logger.LogDependency(dependencyType, dependencyData, isSuccessful: true, startTime: startTime, duration: duration, context: telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal(dependencyType, dependencyTelemetry.Type);
                Assert.Equal(dependencyData.ToString(), dependencyTelemetry.Data);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);

                AssertContainsTelemetryProperty(dependencyTelemetry, "CustomSetting", "Approved");
                AssertContainsTelemetryProperty(dependencyTelemetry, "CustomXml", xml);
            });
        }

        private static string SerializeXml(Order order)
        {
            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(Order));
                serializer.Serialize(writer, order);

                return writer.ToString();
            }
        }

        [Fact]
            public void LogServiceBusDependencyWithNamespace_WithServiceBusDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string entityName = $"entity-name-{Guid.NewGuid()}";
            string endpoint = $"servicebus-endpoint-{Guid.NewGuid()}";
            string dependencyId = $"dependency-{Guid.NewGuid()}";
            const ServiceBusEntityType entityType = ServiceBusEntityType.Unknown;
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Namespace"] = "azure.servicebus.namespace"
            };
            logger.LogServiceBusDependency(endpoint, entityName, isSuccessful: true, startTime, duration, dependencyId, entityType, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure Service Bus", dependencyTelemetry.Type);
                Assert.Equal(entityName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);
                Assert.Equal(dependencyId, dependencyTelemetry.Id);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Namespace", "azure.servicebus.namespace");
                AssertContainsTelemetryProperty(dependencyTelemetry, DependencyTracking.ServiceBus.EntityType, entityType.ToString());
                AssertContainsTelemetryProperty(dependencyTelemetry, DependencyTracking.ServiceBus.Endpoint, endpoint);
            });
        }

        [Fact]
        public void LogServiceBusQueueDependencyWithNamespace_WithServiceBusDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string queueName = $"queue-name-{Guid.NewGuid()}";
            string endpoint = $"servicebus-endpoint-{Guid.NewGuid()}";
            string dependencyId = $"dependency-{Guid.NewGuid()}";
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Namespace"] = "azure.servicebus.namespace"
            };
            logger.LogServiceBusQueueDependency(endpoint, queueName, isSuccessful: true, startTime, duration, dependencyId, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure Service Bus", dependencyTelemetry.Type);
                Assert.Equal(queueName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);
                Assert.Equal(dependencyId, dependencyTelemetry.Id);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Namespace", "azure.servicebus.namespace");
                AssertContainsTelemetryProperty(dependencyTelemetry, DependencyTracking.ServiceBus.EntityType, ServiceBusEntityType.Queue.ToString());
                AssertContainsTelemetryProperty(dependencyTelemetry, DependencyTracking.ServiceBus.Endpoint, endpoint);
            });
        }

        [Fact]
        public void LogServiceBusTopicDependencyWithNamespace_WithServiceBusDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string topicName = $"topic-name-{Guid.NewGuid()}";
            string endpoint = $"servicebus-endpoint-{Guid.NewGuid()}";
            string dependencyId = $"dependency-{Guid.NewGuid()}";
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Namespace"] = "azure.servicebus.namespace"
            };
            logger.LogServiceBusTopicDependency(endpoint, topicName, isSuccessful: true, startTime, duration, dependencyId, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure Service Bus", dependencyTelemetry.Type);
                Assert.Equal(topicName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);
                Assert.Equal(dependencyId, dependencyTelemetry.Id);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Namespace", "azure.servicebus.namespace");
                AssertContainsTelemetryProperty(dependencyTelemetry, DependencyTracking.ServiceBus.EntityType, ServiceBusEntityType.Topic.ToString());
                AssertContainsTelemetryProperty(dependencyTelemetry, DependencyTracking.ServiceBus.Endpoint, endpoint);
            });
        }

        [Fact]
        public void LogBlobStorageDependency_WithTableStorageDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string containerName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Namespace"] = "azure.blobstorage.namespace"
            };
            logger.LogBlobStorageDependency(accountName, containerName, isSuccessful: true, startTime: startTime, duration: duration, context: telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure blob", dependencyTelemetry.Type);
                Assert.Equal(containerName, dependencyTelemetry.Data);
                Assert.Equal(accountName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Namespace", "azure.blobstorage.namespace");
            });
        }

        [Fact]
        public void LogTableStorageDependency_WithTableStorageDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Namespace"] = "azure.tablestorage.namespace"
            };
            logger.LogTableStorageDependency(accountName, tableName, isSuccessful: true, startTime: startTime, duration: duration, context: telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure table", dependencyTelemetry.Type);
                Assert.Equal(tableName, dependencyTelemetry.Data);
                Assert.Equal(accountName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Namespace", "azure.tablestorage.namespace");
            });
        }

        [Fact]
        public void LogEventHubsDependency_WithEventHubsDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            string namespaceName = _bogusGenerator.Finance.AccountName();
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Host"] = "orders.servicebus.windows.net"
            };
            logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful: true, startTime: startTime, duration: duration, context: telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure Event Hubs", dependencyTelemetry.Type);
                Assert.Equal(eventHubName, dependencyTelemetry.Target);
                Assert.Equal(namespaceName, dependencyTelemetry.Data);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Host", "orders.servicebus.windows.net");
            });
        }

        [Fact]
        public void LogIoTHubDependency_WithTableStorageDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string iotHubName = _bogusGenerator.Commerce.ProductName();
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["DeviceName"] = "Sensor #102"
            };
            logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: true, startTime: startTime, duration: duration, context: telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure IoT Hub", dependencyTelemetry.Type);
                Assert.Equal(iotHubName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);

                AssertContainsTelemetryProperty(dependencyTelemetry, "DeviceName", "Sensor #102");
            });
        }

        [Fact]
        public void LogCosmosSqlDependency_WithTableStorageDependency_CreatesDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId));
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Namespace"] = "azure.cosmos.namespace"
            };
            logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful: true, startTime: startTime, duration: duration, context: telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal("Azure DocumentDB", dependencyTelemetry.Type);
                Assert.Equal($"{database}/{container}", dependencyTelemetry.Data);
                Assert.Equal(accountName, dependencyTelemetry.Target);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.True(dependencyTelemetry.Success);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Namespace", "azure.cosmos.namespace");
            });
        }

        [Fact]
        public void LogHttpDependency_WithHttpDependency_CreatesHttpDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, 
                config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                                .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId));

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://localhost/api/v1/health");
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            var telemetryContext = new Dictionary<string, object>
            {
                ["Port"] = "4000"
            };
            logger.LogHttpDependency(request, HttpStatusCode.OK, startTime, duration, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);


            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal(DependencyType.Http.ToString(), dependencyTelemetry.Type);
                Assert.Equal("localhost", dependencyTelemetry.Target);
                Assert.Equal("GET /api/v1/health", dependencyTelemetry.Name);
                Assert.Null(dependencyTelemetry.Data);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.Equal("200", dependencyTelemetry.ResultCode);
                Assert.True(dependencyTelemetry.Success);
                AssertOperationContextForNonRequest(dependencyTelemetry, operationId, transactionId);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Port", "4000");
            });
        }

        [Fact]
        public void LogSqlDependency_WithSqlDependency_CreatesSqlDependencyTelemetry()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, 
                config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                                .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId));
            
            var startTime = DateTimeOffset.UtcNow;
            var duration = TimeSpan.FromSeconds(5);
            string dependencyId = Guid.NewGuid().ToString();
            var telemetryContext = new Dictionary<string, object>
            {
                ["Statement"] = "Query"
            };
            logger.LogSqlDependency("Server", "Database", sqlCommand: "GET", operationName: "Users", isSuccessful: true, startTime: startTime, duration: duration, dependencyId, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, DependencyTracking.DependencyLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var dependencyTelemetry = Assert.IsType<DependencyTelemetry>(telemetry);
                Assert.Equal(DependencyType.Sql.ToString(), dependencyTelemetry.Type);
                Assert.Equal("Server", dependencyTelemetry.Target);
                Assert.Equal("Database/Users", dependencyTelemetry.Name);
                Assert.Equal("GET", dependencyTelemetry.Data);
                Assert.Equal(startTime, dependencyTelemetry.Timestamp);
                Assert.Equal(duration, dependencyTelemetry.Duration);
                Assert.Null(dependencyTelemetry.ResultCode);
                Assert.True(dependencyTelemetry.Success);
                AssertOperationContextForNonRequest(dependencyTelemetry, operationId, transactionId);

                AssertContainsTelemetryProperty(dependencyTelemetry, "Statement", "Query");
            });
        }

        [Fact]
        public void LogCustomEvent_WithEvent_CreatesEventTelemetry()
        {
            // Arrange
            const string eventName = "Order Invoiced";
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink,
                config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                                .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId));

            var telemetryContext = new Dictionary<string, object>
            {
                ["OrderId"] = "ABC",
                ["Vendor"] = "Contoso"
            };

            logger.LogCustomEvent(eventName, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, EventTracking.EventLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var eventTelemetry = Assert.IsType<EventTelemetry>(telemetry);
                Assert.Equal(eventName, eventTelemetry.Name);
                AssertOperationContextForNonRequest(eventTelemetry, operationId, transactionId);
                AssertContainsTelemetryProperty(eventTelemetry, "OrderId", "ABC");
                AssertContainsTelemetryProperty(eventTelemetry, "Vendor", "Contoso");
            });
        }

        [Fact]
        public void LogCustomEvent_WithJsonTelemetryValue_CreatesEventTelemetry()
        {
            // Arrange
            const string eventName = "Order Invoiced";
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink,
                config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                                .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId));

            Order order = OrderGenerator.Generate();
            string json = JsonSerializer.Serialize(order);
            var context = new Dictionary<string, object>
            {
                ["Value"] = json,
                ["OrderId"] = "ABC",
                ["Vendor"] = "Contoso"
            };
            logger.LogCustomEvent(eventName, context);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            Assert.Collection(telemetries, telemetry =>
            {
                var eventTelemetry = Assert.IsType<EventTelemetry>(telemetry);
                Assert.Equal(eventName, eventTelemetry.Name);
                AssertOperationContextForNonRequest(eventTelemetry, operationId, transactionId);
                AssertContainsTelemetryProperty(eventTelemetry, "Value", json);
                AssertContainsTelemetryProperty(eventTelemetry, "OrderId", "ABC");
                AssertContainsTelemetryProperty(eventTelemetry, "Vendor", "Contoso");
            });
        }

        [Fact]
        public void LogException_WithException_CreatesExceptionTelemetry()
        {
            // Arrange
            string platform = $"platform-id-{Guid.NewGuid()}";
            var exception = new PlatformNotSupportedException(platform);
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, 
                config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                                .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId));

            logger.LogCritical(exception, exception.Message);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            Assert.Collection(telemetries, telemetry =>
            {
                var exceptionTelemetryType = Assert.IsType<ExceptionTelemetry>(telemetry);
                Assert.NotNull(exceptionTelemetryType);
                Assert.NotNull(exceptionTelemetryType.Exception);
                Assert.Equal(exception.Message, exceptionTelemetryType.Exception.Message);
                AssertOperationContextForNonRequest(exceptionTelemetryType, operationId, transactionId);
            });
        }

        [Fact]
        public void LogCustomMetric_WithMetric_CreatesMetricTelemetry()
        {
            // Arrange
            const string metricName = "Request stream";
            const double metricValue = 0.13;
            var timestamp = DateTimeOffset.UtcNow;
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink,
                config => config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                                .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId));

            var telemetryContext = new Dictionary<string, object>
            {
                ["Capacity"] = "0.45"
            };
            logger.LogCustomMetric(metricName, metricValue, timestamp, telemetryContext);
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            AssertDoesContainLogProperty(logEvent, MetricTracking.MetricLogEntry);
            Assert.Collection(telemetries, telemetry =>
            {
                var metricTelemetry = Assert.IsType<MetricTelemetry>(telemetry);
                Assert.Equal(metricName, metricTelemetry.Name);
                Assert.Equal(metricValue, metricTelemetry.Sum);
                Assert.Equal(timestamp, metricTelemetry.Timestamp);
                AssertOperationContextForNonRequest(metricTelemetry, operationId, transactionId);

                AssertContainsTelemetryProperty(metricTelemetry, "Capacity", "0.45");
            });
        }

        [Fact]
        public void LogInformationWithPodName_Without_CreatesTraceTelemetryWithPodNameAsRoleInstance()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config =>
            {
                return config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                             .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId)
                             .Enrich.WithProperty(General.ComponentName, "component")
                             .Enrich.WithProperty(Kubernetes.PodName, "pod");
            });

            logger.LogInformation("trace message");
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            Assert.Collection(telemetries, telemetry =>
            {
                var traceTelemetry = Assert.IsType<TraceTelemetry>(telemetry);
                Assert.Equal("trace message", traceTelemetry.Message);
                Assert.Equal(logEvent.Timestamp, traceTelemetry.Timestamp);
                Assert.Equal(SeverityLevel.Information, traceTelemetry.SeverityLevel);
                Assert.Equal("component", traceTelemetry.Context.Cloud.RoleName);
                Assert.Equal("pod", traceTelemetry.Context.Cloud.RoleInstance);
                AssertOperationContextForNonRequest(traceTelemetry, operationId, transactionId);
            });
        }

        [Fact]
        public void LogInformationWithMachineName_Without_CreatesTraceTelemetryWithPodNameAsRoleInstance()
        {
            // Arrange
            var spySink = new InMemoryLogSink();
            string operationId = $"operation-id-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            ILogger logger = CreateLogger(spySink, config =>
            {
                return config.Enrich.WithProperty(ContextProperties.Correlation.OperationId, operationId)
                             .Enrich.WithProperty(ContextProperties.Correlation.TransactionId, transactionId)
                             .Enrich.WithProperty(General.ComponentName, "component")
                             .Enrich.WithProperty(General.MachineName, "machine");
            });

            logger.LogInformation("trace message");
            LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
            Assert.NotNull(logEvent);

            var converter = ApplicationInsightsTelemetryConverter.Create();

            // Act
            IEnumerable<ITelemetry> telemetries = converter.Convert(logEvent, formatProvider: null);

            // Assert
            Assert.Collection(telemetries, telemetry =>
            {
                var traceTelemetry = Assert.IsType<TraceTelemetry>(telemetry);
                Assert.Equal("trace message", traceTelemetry.Message);
                Assert.Equal(logEvent.Timestamp, traceTelemetry.Timestamp);
                Assert.Equal(SeverityLevel.Information, traceTelemetry.SeverityLevel);
                Assert.Equal("component", traceTelemetry.Context.Cloud.RoleName);
                Assert.Equal("machine", traceTelemetry.Context.Cloud.RoleInstance);
                AssertOperationContextForNonRequest(traceTelemetry, operationId, transactionId);
            });
        }

        private static void AssertOperationContextForNonRequest<TTelemetry>(
            TTelemetry telemetry, 
            string operationId,
            string transactionId) where TTelemetry : ITelemetry
        {
            Assert.NotNull(telemetry.Context);
            Assert.NotNull(telemetry.Context.Operation);
            Assert.Equal(operationId, telemetry.Context.Operation.ParentId);
            Assert.Equal(transactionId, telemetry.Context.Operation.Id);
        }

        private static void AssertOperationContextForRequest(
            RequestTelemetry telemetry, 
            string operationId,
            string transactionId,
            string operationParentId)
        {
            Assert.NotNull(telemetry.Context);
            Assert.NotNull(telemetry.Context.Operation);
            Assert.Equal(operationId, telemetry.Id);
            Assert.Equal(transactionId, telemetry.Context.Operation.Id);
            Assert.Equal(operationParentId, telemetry.Context.Operation.ParentId);
        }

        private static ILogger CreateLogger(ILogEventSink sink, Func<LoggerConfiguration, LoggerConfiguration> configureLoggerConfiguration = null)
        {
            LoggerConfiguration config = new LoggerConfiguration().WriteTo.Sink(sink);
            config = configureLoggerConfiguration?.Invoke(config) ?? config;
            Logger logger = config.CreateLogger();

            var factory = new SerilogLoggerFactory(logger);
            return factory.CreateLogger<ApplicationInsightsTelemetryConverterTests>();
        }

        private static HttpRequest CreateStubRequest(HttpMethod httpMethod, string requestScheme, string host, string path)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(req => req.Method).Returns(httpMethod.ToString().ToUpper);
            request.Setup(req => req.Scheme).Returns(requestScheme);
            request.Setup(req => req.Host).Returns(new HostString(host));
            request.Setup(req => req.Path).Returns(path);

            return request.Object;
        }

        private static HttpResponse CreateStubResponse(HttpStatusCode statusCode)
        {
            var response = new Mock<HttpResponse>();
            response.Setup(res => res.StatusCode).Returns((int)statusCode);

            return response.Object;
        }

        private static void AssertDoesContainLogProperty(LogEvent logEvent, string name)
        {
            Assert.True(logEvent.Properties.ContainsKey(name), $"Log event should contain log property with name '{name}'");
        }

        private static void AssertContainsTelemetryProperty(ISupportProperties telemetry, string key, string value)
        {
            Assert.True(
                telemetry.Properties.Contains(new KeyValuePair<string, string>(key, value)), 
                $"Value ({value}) not in telemetry context ({String.Join(", ", telemetry.Properties.Select(item => $"[{item.Key}] = {item.Value}"))})");
        }
    }
}
