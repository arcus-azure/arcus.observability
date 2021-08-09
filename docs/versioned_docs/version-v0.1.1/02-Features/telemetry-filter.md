---
title: "Telemetry Filters"
layout: default
---

# Telemetry Filters

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Filters -Version 0.1.1
```

## Telemetry Type Filter

This [Serilog filter](https://github.com/serilog/serilog/wiki/Enrichment) allows you to filter out different a specific type of telemetry.

```csharp
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Filters;
using Serilog.Core;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsights("<key>")
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Events))
    .CreateLogger();
```

The filter can also be used to reduce telemetry for multiple types by chaining them:

```csharp
ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsights("<key>")
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Events))
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
    .CreateLogger();
```

Alternatively, you can explicitly specify if it should track telemetry or not based on the application configuration has to be tracked or not :

```csharp
var trackDependencies = configuration["telemetry:depenencies:isEnabled"];
ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsights("<key>")
    .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency, isTrackingEnabled: bool.Parse(trackDependencies)))
    .CreateLogger();
```


