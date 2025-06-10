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

Tracking the outgoing dependency:

```csharp
var durationMeasurement = new StopWatch();

// Start measuring
durationMeasurement.Start();
var startTime = DateTimeOffset.UtcNow;

var trackingId = "75D298F7-99FF-4BB8-8019-230974EB6D1E";

logger.LogDependency(dependencyName, dependencyData, isSuccessful: false, measurement, dependencyId: trackingId, telemetryContext);
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