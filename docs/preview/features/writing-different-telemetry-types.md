---
title: "Write different telemetry types"
layout: default
---

# Write different telemetry types

Logs are a great way to gain insights, but sometimes they are not the best approach for the job.

We provide the capability to track the following telemetry types on top of ILogger with good support on Serilog:

- [Dependencies](#dependencies)
- [Events](#events)
- [Metrics](#metrics)
- [Requests](#requests)

For most optimal output, we recommend using our [Azure Application Insights sink](/features/sinks/azure-application-insights).

**We highly encourage to provide contextual information to all your telemetry** to make it more powerful and support this for all telemetry types.

> :bulb: For sake of simplicity we have not included how to track contextual information, for more information see [our documentation](/features/making-telemetry-more-powerful).

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Core
```

## Dependencies

Dependencies allow you to track how your external dependencies are doing to give you insights on performance and error rate.

We provide support for the following dependencies:

- [Azure Blob Storage](#measuring-azure-blob-storage-dependencies)
- [Azure Cosmos DB](#measuring-azure-cosmos-db-dependencies)
- [Azure Event Hubs](#measuring-azure-event-hubs-dependencies)
- [Azure IoT Hub](#measuring-azure-iot-hub-dependencies)
- [Azure Service Bus](#measuring-azure-service-bus-dependencies)
- [Azure Table Storage](#measuring-azure-table-storage-dependencies)
- [Custom](#measuring-custom-dependencies)
- [HTTP](#measuring-http-dependencies)
- [SQL](#measuring-sql-dependencies)

Since measuring dependencies can add some noise in your code, we've introduced `DependencyMeasurement` to make it simpler. ([docs](#making-it-easier-to-measure-dependencies))

### Measuring Azure Blob Storage dependencies

We allow you to measure Azure Blob Storage dependencies.

Here is how you can report a dependency call:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

_logger.LogBlobStorageDependency(accountName: "multimedia", containerName: "images", isSuccessful: true, startTime, durationMeasurement.Elapsed);
// Output: "Dependency Azure blob multimedia named images in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

### Measuring Azure Cosmos DB dependencies

We allow you to measure Azure Cosmos dependencies.

Here is how you can report a dependency call:

**Cosmos SQL**

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

_logger.LogCosmosSqlDependency(accountName: "administration", database: "docs", container: "purchases", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "Dependency Azure DocumentDB docs/purchases named administration in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

### Measuring Azure Event Hubs dependencies

We allow you to measure Azure Event Hubs dependencies.

Here is how you can report a dependency call:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

_logger.LogEventHubsDependency(namespaceName: "be.sensors.contoso", eventHubName: "temperature", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "Dependency Azure Event Hubs be.sensors.contoso named temerature in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

### Measuring Azure IoT Hub dependencies

We allow you to measure Azure IoT Hub dependencies.

Here is how you can report a dependency call:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

_logger.logger.LogIotHubDependency(iotHubName: "sensors", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "Dependency Azure IoT Hub named sensors in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

### Measuring Azure Service Bus dependencies

We allow you to measure Azure Service Bus dependencies for both queues & topics.

Here is how you can report an Azure Service Bus Queue dependency:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

_logger.LogServiceBusQueueDependency(queueName: "ordersqueue", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "Dependency Azure Service Bus Queue named ordersqueue in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

Note that we have an `LogServiceBusTopicDependency` to log dependency logs for an Azure Service Bus Topic and an `LogServiceBusDependency` to log Azure Service Bus logs where the entity type is not known.

### Measuring Azure Table Storage Dependencies

We allow you to measure Azure Table Storage dependencies.

Here is how you can report a dependency call:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

_logger.LogTableStorageDependency(accountName: "orderAccount", tableName: "orders", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "Dependency Azure table orders named orderAccount in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

### Measuring HTTP dependencies

Here is how you can report a HTTP dependency:

```csharp
var durationMeasurement = new Stopwatch();

// Create request
var request = new HttpRequestMessage(HttpMethod.Post, "http://requestbin.net/r/ujxglouj")
{
    Content = new StringContent("{\"message\":\"Hello World!\"")
};

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;
// Send request to dependant service
var response = await httpClient.SendAsync(request);

_logger.LogHttpDependency(request, statusCode: response.StatusCode, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "HTTP Dependency requestbin.net for POST /r/ujxglouj completed with 200 in 00:00:00.2521801 at 03/23/2020 09:56:31 +00:00 (Successful: True - Context: )"
```

### Measuring SQL dependencies

Here is how you can report a SQL dependency:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

// Interact with database
var products = await _repository.GetProducts();

_logger.LogSqlDependency("sample-server", "sample-database", "my-table", "get-products", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "SQL Dependency sample-server for sample-database/my-table for operation get-products in 00:00:01.2396312 at 03/23/2020 09:32:02 +00:00 (Successful: True - Context: )"
```

Or alternatively, when one already got the SQL connection string, you can use the overload that takes this directly:

```csharp
string connectionString = "Server=sample-server;Database=sample-database;User=admin;Password=123";
var durationMeasurement = new Stopwatch();

// Start measuring
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

// Interact with database
var products = await _repository.GetProducts();

_logger.LogSqlDependency(connectionString, "my-table", "get-products", isSuccessful: true, measurement: measurement);
// Output: "SQL Dependency sample-server for sample-database/my-table for operation get-products in 00:00:01.2396312 at 03/23/2020 09:32:02 +00:00 (Successful: True - Context: )"
```

### Measuring custom dependencies

Here is how you can areport a custom depenency:

```csharp
var durationMeasurement = new Stopwatch();

// Start measuring
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

string dependencyName = "SendGrid";
object dependencyData = "http://my.sendgrid.uri/"

_logger.LogDependency("SendGrid", dependencyData, isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: "Dependency SendGrid http://my.sendgrid.uri/ in 00:00:01.2396312 at 03/23/2020 09:32:02 +00:00 (Successful: True - Context: )"
```

### Making it easier to measure dependencies

Measuring dependencies means you need to keep track of how long the action took and when it started.

Here's a small example:

```csharp
var durationMeasurement = new Stopwatch();
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

// Do action

/// Track dependency
string dependencyName = "SendGrid";
object dependencyData = "https://my.sendgrid.uri/";
_logger.LogDependency("SendGrid", dependencyData, isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed, context: telemetryContext);
```

However, by using `DependencyMeasurement.Start()` we take care of the measuring aspect:

```csharp
// Start measuring
using (var measurement = DependencyMeasurement.Start())
{
    // Do Action

    // Track dependency
    string dependencyName = "SendGrid";
    object dependencyData = "https://my.sendgrid.uri/";
    _logger.LogDependency(dependencyName, dependencyData, isSuccessful: true, startTime: measurement, context: telemetryContext);
}
```

Failures during the interaction with the tracked dependency can be controlled by the passed-allong `boolean`:

```csharp
string dependencyName = "SendGrid";
object dependencyData = "https://my.sendgrid.uri";

try
{
    // Interact with SendGrid...
    // Done!

    _logger.LogDependency(dependencyName, dependencyData, isSuccessful: true, startTime: measurement, context: telemetryContext);
}
catch (Exception exception)
{
    _logger.LogError(exception, "Failed to interact with SendGrid");
    _logger.LogDependency(dependencyName, dependencyData, isSuccessful: false, startTime: measurement, context: telemetryContext);
}
```

## Events

Events allow you to report custom events which are a great way to track business-related events.

Here is how you can report an `Order Created` event:

```csharp
logger.LogEvent("Order Created");
// Output: "Events Order Created (Context: )"
```

### Security Events

Some events are considered "security events" when they relate to possible malicious activity, authentication, input validation...

Here is how an invalid `Order` can be reported:

```csharp
loger.LogSecurityEvent("Invalid Order");
// Output: "Events Invalid Order (Context: )"
```

## Metrics

Metrics allow you to report custom metrics which allow you to give insights on application-specific metrics.

Here is how you can report an `Invoice Received` metric:

```csharp
logger.LogMetric("Invoice Received", 133.37, telemetryContext);
// Output: "Metric Invoice Received: 133.37 (Context: )"
```

## Requests

Requests allow you to keep track of the HTTP requests that are performed against your API and what the response was that was sent out.

Here is how you can keep track of requests:

```csharp
// Determine calling tenant
string tenantName = "Unknown";
if (httpContext.Request?.Headers?.ContainsKey("X-Tenant") == true)
{
    tenantName = httpContext.Request.Headers["X-Tenant"];
}

var stopWatch = Stopwatch.StartNew();

// Perform action that creates a response, in this case call next middleware in the chain.
await _next(httpContext);

logger.LogRequest(httpContext.Request, httpContext.Response, stopWatch.Elapsed);
// Output: "HTTP Request GET http://localhost:5000//weatherforecast completed with 200 in 00:00:00.0191554 at 03/23/2020 10:12:55 +00:00 - (Context: )"
```

[&larr; back](/)
