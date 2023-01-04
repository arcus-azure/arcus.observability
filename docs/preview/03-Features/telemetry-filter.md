---
title: "Telemetry Filters"
layout: default
---

# Telemetry Filters

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Filters
```

## Telemetry Type Filter

In certain scenarios, the cost of having too much telemetry or having a certain type of telemetry in your logging can be too high to be useful. We could, for example, filter out in container logs the Request, Metrics and Dependencies, and discard all the other types to make the logging output more readable and cost-effective.

This [Serilog filter](https://github.com/serilog/serilog/wiki/Enrichment) allows you to filter out different a specific type of telemetry.

```csharp
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Filters;
using Serilog.Core;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>")
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Events))
    .CreateLogger();
```

The filter can also be used to reduce telemetry for multiple types by chaining them:

```csharp
ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>")
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Events))
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
    .CreateLogger();
```

Alternatively, you can explicitly specify if it should track telemetry or not based on the application configuration has to be tracked or not:

```csharp
var trackDependencies = configuration["telemetry:dependencies:isEnabled"];
ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>")
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency, isTrackingEnabled: bool.Parse(trackDependencies)))
    .CreateLogger();
```


