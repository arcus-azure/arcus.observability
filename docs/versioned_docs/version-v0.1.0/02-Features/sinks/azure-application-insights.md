---
title: "Azure Application Insights Sink"
layout: default
---

# Azure Application Insights Sink

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights -Version 0.1.0
```

## What is it?

The Azure Application Insights sink is an extension of the [official Application Insights sink](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/) that allows you to not only emit traces or events, but the whole Application Insights suite of telemetry types - Traces, Dependencies, Events, Requests & Metrics.

You can easily configure the sink by providing the Azure Application Insights key:

```csharp
using Serilog;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsights("<key>")
    .CreateLogger();
```

Alternatively, you can override the default minimum log level to reduce amount of telemetry being tracked :

```csharp
using Serilog;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsights("<key>", restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();
```


