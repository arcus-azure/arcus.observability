---
title: "Write different telemetry types"
layout: default
---

# Write different telemetry types

Logs are a great way to gain insights, but sometimes they are not the best approach for the job.

We provide the capability to track the following telemetry types on top of ILogger with good support on Serilog.
For most optimal output, we recommend using our [Azure Application Insights sink](./sinks/azure-application-insights.md).

**We highly encourage to provide contextual information to all your telemetry** to make it more powerful and support this for all telemetry types.

> :bulb: For sake of simplicity we have not included how to track contextual information, for more information see [our documentation](./making-telemetry-more-powerful.md).

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Core
```

## Dependencies

Dependencies allow you to track how your external dependencies are doing to give you insights on performance and error rate.

Since measuring dependencies can add some noise in your code, we've introduced `DependencyMeasurement` to make it simpler. ([docs](#making-it-easier-to-measure-dependencies))
Linking service-to-service correlation can be hard, this can be made easier with including dependency ID's. ([docs](#making-it-easier-to-link-services))

### Measuring Azure Blob Storage dependencies

We allow you to measure Azure Blob Storage dependencies.

Here is how you can report a dependency call:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogBlobStorageDependency(accountName: "multimedia", containerName: "images", isSuccessful: true, startTime, durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure blob",  "DependencyName": "images", "TargetName": "multimedia", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring Azure Cosmos DB dependencies

We allow you to measure Azure Cosmos dependencies.

Here is how you can report a dependency call:

**Cosmos SQL**

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogCosmosSqlDependency(accountName: "administration", database: "docs", container: "purchases", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure DocumentDB", "DependencyData": "docs/purchases", "TargetName": "administration", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful" true, "Context": {}}
```

### Measuring Azure Event Hubs dependencies

We allow you to measure Azure Event Hubs dependencies.

Here is how you can report a dependency call:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogEventHubsDependency(namespaceName: "be.sensors.contoso", eventHubName: "temperature", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure Event Hubs", "DependencyData": "be.sensors.contoso", "TargetName": "temperature", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring Azure IoT Hub dependencies

We allow you to measure Azure IoT Hub dependencies.

**Example**

Here is how you can report a dependency call:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogIotHubDependency(iotHubName: "sensors", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure IoT Hub", "TargetName": "sensors", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

Or, alternatively you can pass along the IoT connection string itself so the host name will be selected for you.

**Installation**

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.IoT
```

**Example**

Here is how you can report a dependency call:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogIotHubDependency(iotHubConnectionString: "Hostname=sensors;", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure IoT Hub", "TargetName": "sensors", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring Azure Key Vault dependencies

We allow you to measure Azure Key vault dependencies.

**Example**

Here is how you can report a dependency call:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new StopWatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.AzureKeyVaultDependency(vaultUri: "https://my-secret-store.vault.azure.net", secretName: "ServiceBus-ConnectionString", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure key vault", "DependencyData": "ServiceBus-ConnectionString", "TargetName": "https://my-secret-store.vault.azure.net", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring Azure Search dependencies

We allow you to measure Azure Search dependencies for cognitive services.

Here is how you can report an Azure Search dependency:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogAzureSearchDependency(searchServiceName: "orders-search", operationName: "get-orders", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure Search", "DependencyData": "get-orders", "TargetName": "orders-search", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring Azure Service Bus dependencies

We allow you to measure Azure Service Bus dependencies for both queues & topics.

Here is how you can report an Azure Service Bus Queue dependency:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow

logger.LogServiceBusQueueDependency(queueName: "ordersqueue", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure Service Bus", "TargetName": "ordersqueue", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {"EntityType": "Queue"}}
```

Note that we have an `LogServiceBusTopicDependency` to log dependency logs for an Azure Service Bus Topic and an `LogServiceBusDependency` to log Azure Service Bus logs where the entity type is not known.

### Measuring Azure Table Storage Dependencies

We allow you to measure Azure Table Storage dependencies.

Here is how you can report a dependency call:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

logger.LogTableStorageDependency(accountName: "orderAccount", tableName: "orders", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Azure table", "DependencyData": "orders", "TargetName": "orderAccount", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring HTTP dependencies

Here is how you can report a HTTP dependency:

```csharp
using Microsoft.Extensions.Logging;

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

logger.LogHttpDependency(request, statusCode: response.StatusCode, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType" "Http", "DependencyName": "POST /r/ujxglouj", "TargetName": "requestbin.net", "ResultCode": 200, "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring SQL dependencies

Here is how you can report a SQL dependency:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

// Interact with database
var products = await _repository.GetProducts();

logger.LogSqlDependency("Company SQL server", "Stock Database", "SELECT ProductName FROM Products", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "Sql", "DependencyName": "Stock Database", "DependencyData": "SELECT ProductName FROM Products", "TargetName": "Company SQL Server", "Duration": "00:00:01.2396312", "StartTime": "03/23/2020 09:32:02 +00:00", "IsSuccessful": true, "Context": {}}
```

Or alternatively, when one already got the SQL connection string, you can use the overload that takes this directly:

**Installation**

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Sql
```

**Example**

```csharp
using Microsoft.Extensions.Logging;

string connectionString = "Server=Company SQL Server;Database=Stock Database;User=admin;Password=123";
var durationMeasurement = new Stopwatch();

// Start measuring
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

// Interact with database
var products = await _repository.GetProducts();

logger.LogSqlDependency(connectionString, "SELECT ProductName FROM Products", isSuccessful: true, startTime, durationMeasurement.Elapsed);
// Output: {"DependencyType": "Sql", "DependencyName": "Stock Database", "DependencyData": "SELECT ProductName FROM Proucts", "TargetName": "Company SQL Server", "Duration": "00:00:01.2396312", "StartTime": "03/23/2020 09:32:02 +00:00", "IsSuccessful": true, "Context": {}}
```

### Measuring custom dependencies

Here is how you can measure a custom dependency:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();

// Start measuring
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

string dependencyName = "SendGrid";
object dependencyData = "http://my.sendgrid.uri/"

logger.LogDependency("SendGrid", dependencyData, isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed);
// Output: {"DependencyType": "SendGrid", "DependencyData": "http://my.sendgrid.uri/", "Duration": "00:00:01.2396312", "StartTime": "03/23/2020 09:32:02 +00:00", "IsSuccessful": true, "Context": {}}
```

### Making it easier to measure telemetry

Measuring dependencies or requests means you need to keep track of how long the action took and when it started.
The `Arcus.Observability.Telemetry.Core` library provides an easy way to accomplish this.

Here's a small example:

```csharp
using Microsoft.Extensions.Logging;

var durationMeasurement = new Stopwatch();
var startTime = DateTimeOffset.UtcNow;
durationMeasurement.Start();

// Do action

/// Track dependency
string dependencyName = "SendGrid";
object dependencyData = "https://my.sendgrid.uri/";
logger.LogDependency("SendGrid", dependencyData, isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed, context: telemetryContext);
```

#### Making it easier to measure dependencies

By using `DurationMeasurement.Start()` we take care of the measuring aspect:

```csharp
using Arcus.Observability.Telemetry.Core;
using Microsoft.Extensions.Logging;

// Start measuring
using (var measurement = DurationMeasurement.Start())
{
    // Do Action

    // Track dependency
    string dependencyName = "SendGrid";
    object dependencyData = "https://my.sendgrid.uri/";
    logger.LogDependency(dependencyName, dependencyData, isSuccessful: true, measurement, telemetryContext);
}
```

Failures during the interaction with the dependency can be controlled by passing `isSuccessful`:

```csharp
using Arcus.Observability.Telemetry.Core;
using Microsoft.Extensions.Logging;

string dependencyName = "SendGrid";
object dependencyData = "https://my.sendgrid.uri";

try
{
    // Interact with SendGrid...
    // Done!

    logger.LogDependency(dependencyName, dependencyData, isSuccessful: true, measurement, telemetryContext);
}
catch (Exception exception)
{
    logger.LogError(exception, "Failed to interact with SendGrid");
    logger.LogDependency(dependencyName, dependencyData, isSuccessful: false, measurement, telemetryContext);
}
```

#### Making it easier to measure requests

By using `DurationMeasurement.Start()` we take care of the measuring aspect:

```csharp
using Arcus.Observability.Telemetry.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

HttpRequest request = ...
HttpResponse response = ...

// Start measuring
using (var measurement = DurationMeasurement.Start())
{
    // Process message

    // Track request
    logger.LogRequest(request, response, measurement, telemetryContext);
}
```

### Making it easier to link services

Service-to-service correlation requires linkage between tracked dependencies (outgoing) and requests (incoming).
Tracking any kind of dependency with the library has the possibility to provide an dependency ID.

To link the request (incoming) with the dependency (outgoing), the request needs to include this dependency ID in its tracking (dependency ID = request's parent ID) so that we now which dependency triggered the request.
For more information, see how to do this in a Web API and [Azure Service Bus](#incoming-azure-service-bus-requests) context.

Tracking the outgoing dependency:

```csharp
var durationMeasurement = new StopWatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

var trackingId = "75D298F7-99FF-4BB8-8019-230974EB6D1E";

logger.AzureKeyVaultDependency(
    vaultUri: "https://my-secret-store.vault.azure.net", 
    secretName: "ServiceBus-ConnectionString", 
    isSuccessful: true, 
    startTime: startTime, 
    duration: durationMeasurement.Elapsed,
    dependencyId: trackingId);

// Output: {"DependencyType": "Azure key vault", "DependencyId": "75D298F7-99FF-4BB8-8019-230974EB6D1E", "DependencyData": "ServiceBus-ConnectionString", "TargetName": "https://my-secret-store.vault.azure.net", "Duration": "00:00:00.2521801", "StartTime": "03/23/2020 09:56:31 +00:00", "IsSuccessful": true, "Context": {}}
```

## Events

Events allow you to report custom events which are a great way to track business-related events.

Here is how you can report an `Order Created` event:

```csharp
using Microsoft.Extensions.Logging;

logger.LogCustomEvent("Order Created");
// Output: {"EventName": "Order Created", "Context": {}}
```

### Security Events

Some events are considered "security events" when they relate to possible malicious activity, authentication, input validation...

Here is how an invalid `Order` can be reported:

```csharp
using Microsoft.Extensions.Logging;

loger.LogSecurityEvent("Invalid Order");
// Output: {"EventName": "Invalid Order", "Context": {"EventType": "Security"}}
```

## Metrics

Metrics allow you to report custom metrics which allow you to give insights on application-specific metrics.

Here is how you can report an `Invoices Received` metric:

```csharp
using Microsoft.Extensions.Logging;

logger.LogCustomMetric("Invoices Received", 133);
// Output: {"MetricName": "Invoices Received", "MetricValue": 133, "Timestamp": "03/23/2020 09:32:02 +00:00", "Context: {[TelemetryType, Metric]}}
```

## Requests

### Incoming Azure Service Bus requests
Requests allow you to keep track of incoming Azure Service Bus messages on a queue or topic.

Here is how you can log an Azure Service Bus queue request on a message that's being processed:

```csharp
using Microsoft.Extensions.Logging;

bool isSuccessful = false;

// Start measuring.
using (var measurement = DurationMeasurement.Start())
{
    try
    {
        // Processing message.

        // End processing.
        
        isSuccessful = true;
    }
    finally
    {
        logger.LogServiceBusQueueRequest("<my-queue-namespace>.servicebus.windows.net", "<my-queue-name>", "<operation-name>", isSuccessful, measurement);
        // Output: Azure Service Bus from <operation-name> completed in 0.00:12:20.8290760 at 2021-10-26T05:36:03.6067975 +02:00 - (IsSuccessful: True, Context: {[ServiceBus-Endpoint, <my-queue-namespace>.servicebus.windows.net]; [ServiceBus-Entity, <my-queue-name>]; [ServiceBus-EntityType, Queue]; [TelemetryType, Request]})
    }
}
```

We provide support for all Azure Service Bus entity types such as queues, topics and subscriptions. 
All these types can be tracked by passing along the full Azure Service namespace, or with providing the namespace name and the Azure cloud separately.

```csharp

DependencyMeasurement measurement = ...

// Tracking Azure Service Bus topics.
// ----------------------------------

// Providing the full Azure Service Bus topic namespace.
logger.LogServiceBusTopicRequest("<my-topic-namespace>.servicebus.windows.net", "<my-topic-name>", "<subscription-name>", "<operation-name>", isSuccessful: true, measurement);

// Providing the Azure Service Bus topic name and Azure cloud separately.
logger.LogServiceBusTopicRequestWithSuffix("<my-topic-namespace-name>", serviceBusNamespaceSuffix: ".servicebus.windows.net", "<my-topic-name>", "<subscription-name>", "<operation-name>", isSuccessful: true, measurement);


// Tracking general Azure Service Bus requests.
// --------------------------------------------

// Providing the full Azure Service Bus topic namespace.
logger.LogServiceBusRequest("<my-topic-namespace>.servicebus.windows.net", "<my-topic-name>", "<subscription-name>", "<operation-name>", isSuccessful: true, measurement, ServiceBusEntityType.Topic);

// Providing the Azure Service Bus queue namespace name and Azure cloud separately.
logger.LogServiceBusQueueRequestWithSuffix("<my-queue-namespace-name>", serviceBusNamespaceSuffix: ".servicebus.windows.net", "<my-queue-name>", "<operation-name>", isSuccessful: true, measurement, ServiceBusEntityType.Queue);
```

### Incoming Azure EventHubs requests
Requests allow you to keep track of incoming Azure EventHubs event messages.

Here is how you can log an Azure EventHubs request on an event that's being processed:

```csharp
using Microsoft.Extensions.Logging;

bool isSuccessful = false;

// Start measuring.
using (var measurement = DurationMeasurement.Start())
{
    try
    {
        // Processing message.

        // End processing.
        
        isSuccessful = true;
    }
    finally
    {
        logger.LogEventHubsRequest("<my-eventhubs-namespace>", "<my-eventhubs-name>", isSuccessful, measurement);
        // Output: Azure EventHubs from Process completed in 0.00:12:20.8290760 at 2021-10-26T05:36:03.6067975 +02:00 - (IsSuccessful: True, Context: {[EventHubs-Namespace, <my-eventhubs-namespace>]; [EventHubs-Name, <my-eventhubs-name>]; [EventHubs-ConsumerGroup, $Default]; [TelemetryType, Request]})
    }
}
```

We provide overloads to configure the Azure EventHubs consumer group (default: `$Default`) and a functional operation name (default: `Process`).

### Incoming HTTP requests in API's
Requests allow you to keep track of the HTTP requests that are performed against your API and what the response was that was sent out.

**Installation**

If you want to track the `HttpRequest` and `HttpResponse` of an ASP.NET Core project, you'll have to install an additional package to include these ASP.NET Core dependencies:

```shell
PM > Install-Package Arcus.Observability.Telemetry.AspNetCore
```

**Example**

Here is how you can keep track of requests:

```csharp
using Microsoft.Extensions.Logging;

// Determine calling tenant
string tenantName = "Unknown";
if (httpContext.Request?.Headers?.ContainsKey("X-Tenant") == true)
{
    tenantName = httpContext.Request.Headers["X-Tenant"];
}

// Start tracking request.
using (var measurement = DurationMeasurement.Start())
{
    // Perform action that creates a response, in this case call next middleware in the chain.
    await _next(httpContext);

    logger.LogRequest(httpContext.Request, httpContext.Response, measurement);
    // Output: {"RequestMethod": "GET", "RequestHost": "http://localhost:5000/", "RequestUri": "http://localhost:5000/weatherforecast", "ResponseStatusCode": 200, "RequestDuration": "00:00:00.0191554", "RequestTime": "03/23/2020 10:12:55 +00:00", "Context": {}}
}
```

> 💡 Note that [Arcus Web API request tracking middleware](https://webapi.arcus-azure.net/features/logging#logging-incoming-requests) can already do this for you in a ASP.NET Core application

### Incoming HTTP requests in Azure Function HTTP trigger
Requests allow you to keep track of the HTTP requests that are performed against your Azure Function HTTP trigger and what the response was that was sent out.

**Installation**

If you want to track the `HttpRequestData` of an isolated Azure Functions HTTP trigger project, you'll have to install an additional package to include these dependencies:

```shell
PM > Install-Package Arcus.Observability.Telemetry.AzureFunctions
```

**Example**

Here is how you can keep track of requests:

```csharp
using Sytem.Net;
using Microsoft.Extensions.Logging;

public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/order")] HttpRequestData request,
    FunctionContext executionContext)
{
    var statusCode = default(HttpStatusCode);

    // Start tracking request.
    using (var measurement = DurationMeasurement.Start())
    {
        try
        {
            // Processing...

            statusCode = HttpStatusCode.Ok;
            return request.CreateResponse(statusCode)
        }
        finally
        {
            logger.LogRequest(request, statusCode, measurement);
            // Output: {"RequestMethod": "POST", "RequestHost": "http://localhost:5000/", "RequestUri": "http://localhost:5000/v1/order", "ResponseStatusCode": 200, "RequestDuration": "00:00:00.0191554", "RequestTime": "03/23/2020 10:12:55 +00:00", "Context": {}}
        }
    }
}
```

### Incoming custom requests
Requests allow you to keep track of incoming messages. We provide an extension to track type of requests that aren't out-of-the-box so you can track your custom systems.

Here is how you can log a custom request on an event that's being processed:

```csharp
using Microsoft.Extensions.Logging;

bool isSuccessful = false;

// Start measuring.
using (var measurement = DurationMeasurement.Start())
{
    try
    {
        // Processing message.

        // End processing.
        
        isSuccessful = true;
    }
    finally
    {
        logger.LogCustomRequest("<my-request-source>", "<operation-name>", isSuccessful, measurement);
        // Output: Custom <my-request-source> from Process completed in 0.00:12:20.8290760 at 2021-10-26T05:36:03.6067975 +02:00 - (IsSuccessful: True, Context: {[TelemetryType, Request]})
    }
}
```

The `<my-request-source>` will reflect the `Source` in Application Insights telemetry. This is set automatically in our HTTP, Azure Service Bus, Azure EventHubs, etc. requests but is configurable when you track custom requests.
We provide overloads to configure the functional operation name (default: `Process`).

## Traces & Exceptions
Application Insights telemetry traces and exceptions are log messages not directly linked by an incoming request, outgoing dependency, or metric. 
These traces are also linked with correlation and are therefore part of the whole application component in Application Insights.

Traces and exceptions can be logged with the general Microsoft logging extensions like: `LogInformation`, `LogWarning`, `LogError`...
To help with logging useful traces, we provide several overloads on these existing extensions to pass in additional contextual information.

> ⚠ Note that log messages that gets an exception passed-in, are tracked as exceptions in Application Insights, and not as traces.

```csharp
using Microsoft.Extensions.Logging;

var telemetryContext = new Dictionary<string, object>
{
    ["Order_Id"] = "abc-def",
    ["Order_Date"] = DateTimeOffset.UtcNow
};

ILogger logger = ...

// Informational messages (traces)
logger.LogInformation("This is an informational message!", telemetryContext);
// > Result in trace with telemetry context.

logger.LogInformation("this is an informational message with an {Argument}!", telemetryContext, "The argument");
// > Result in trace with telemetry context.


// Error messages (traces / exceptions)
logger.LogError("This is an error message!", telemetryContext);
// > Result in trace with telemetry context.

var exception = new ApplicationException("Something happened in the application!");
logger.LogError(exception, "This is an error message!", telemetryContext);
// > Result in exception with telemetry context.
```