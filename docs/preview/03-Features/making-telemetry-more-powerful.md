---
title: "Making telemetry more powerful"
layout: default
---

# Making telemetry more powerful

## Providing contextual information

In order to make telemetry more powerful we **highly recommend providing contextual information around what the situation is of your application**. That's why every telemetry type that you can write, allows you to provide context in the form of a dictionary.

```csharp
using Microsoft.Extensions.Logging;

// Provide context around event
var telemetryContext = new Dictionary<string, object>
{
    {"Customer", "Arcus"},
    {"OrderId", "ABC"},
};

logger.LogCustomEvent("Order Created", telemetryContext);
// Output: "Events Order Created (Context: [Customer, Arcus], [OrderId, ABC])"
```

By doing so, you'll be able to interact more efficient with your logs by filtering, searching, ... on it.

We support this for all [telemetry types that you can write](./writing-different-telemetry-types.md).

### Seeing the power in action

Let's use an example - When measuring a metric you get an understanding of the count, in our case the number of orders received:

```csharp
using Microsoft.Extensions.Logging;

logger.LogCustomMetric("Orders Received", 133);
// Log output: "Metric Orders Received: 133 (Context: )"
```

If we output this to Azure Application Insights as a metric similar to our example:
![Single-dimension Metric](/media/single-dimensional-metric.png)

However, you can very easily provide additional context, allowing you to get an understanding of the number of orders received and annotate it with the vendor information.

```csharp
using Microsoft.Extensions.Logging;

var telemetryContext = new Dictionary<string, object>
{
    { "Customer", "Contoso"},
};

logger.LogCustomMetric("Orders Received", 133, telemetryContext);
// Log output: "Metric Orders Received: 133 (Context: [Customer, Contoso])"
```

The outputted telemetry will contain that information and depending on the sink that you are using it's even going to be more powerful.

For example, when using Azure Application Insights your metric will evolve from a single-dimensional metric to multi-dimensional metrics allowing you to get the total number of orders, get the number of orders per vendor or filter the metric to one specific vendor.

Here we are using our multi-dimensional metric and splitting it per customer to get more detailed insights:

![Multi-dimension Metric](/media/multi-dimensional-metrics.png)
