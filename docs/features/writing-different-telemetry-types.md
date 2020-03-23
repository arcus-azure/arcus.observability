---
title: "Write different telemetry types"
layout: default
---

# Write different telemetry types

Logs are a great way to gain insights, but sometimes they are not the best approach for the job.

We provide the capability to track the following telemetry types on top of ILogger with good support on Serilog:

- Dependencies
- [Events](#events)
- [Metrics](#metrics)
- [Requests](#requests)

For most optimal output, we recommend using our [Azure Application Insights sink](/features/sinks/azure-application-insights).

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Core
```

## Events

Events allow you to report custom events which are a great way to track business-related events.

Here is how you can report an `Order Created` event:

```csharp
logger.LogEvent("Order Created");
// Output: "Events Order Created (Context: )"
```

Contextual information is essential, that's why we provide an overload to give more information about the event:

```csharp
// Provide context around event
var telemetryContext = new Dictionary<string, object>
{
    {"Customer", "Arcus"},
    {"OrderId", "ABC"},
};

logger.LogEvent("Order Created", telemetryContext);
// Output: "Events Order Created (Context: [Customer, Arcus], [OrderId, ABC])"
```

### Security Events

Some events are considered "security events" when they relate to possible malicious activity, authentication, input validation...

Here is how an invalid `Order` can be reported:

```csharp
// Provide context around security event
var telemetryContext = new Dictionary<string, object>
{
    {"OrderId", "OrderId was not in correct format"}
};

loger.LogSecurityEvent("Invalid Order", telemetryContext);
// Output: "Events Invalid Order (Context: [EventType, Security], [OrderId, OrderId was not in correct format])"
```

## Metrics

Metrics allow you to report custom metrics which allow you to give insights on application-specific metrics.

Here is how you can report an `Invoice Received` metric:

```csharp
// Provide context around metric
var telemetryContext = new Dictionary<string, object>
{
    { "InvoiceId", "ABC"},
    { "Vendor", "Contoso"},
};

logger.LogMetric("Invoice Received", 133.37, telemetryContext);
// Output: "Metric Invoice Received: 133.37 (Context: [InvoiceId, ABC], [Vendor, Contoso])"
```

By using contextual information, you can create powerful metrics. When writing to Application Insights, for example, which will report the `Invoice Received` metric as [multi-dimensional metrics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/data-platform-metrics#multi-dimensional-metrics) which allow you to filter the metric based on its context.

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

// Provide context around request
var telemetryContext = new Dictionary<string, object>
{
    { "Tenant", tenantName},
};

var stopWatch = Stopwatch.StartNew();

// Perform action that creates a response, in this case call next middleware in the chain.
await _next(httpContext);

logger.LogRequest(httpContext.Request, httpContext.Response, stopWatch.Elapsed, telemetryContext);
// Output: "HTTP Request GET http://localhost:5000//weatherforecast completed with 200 in 00:00:00.0191554 at 03/23/2020 10:12:55 +00:00 - (Context: [Tenant, Contoso])"
```

[&larr; back](/)
