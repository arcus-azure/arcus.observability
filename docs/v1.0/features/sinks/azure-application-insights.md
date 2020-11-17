---
title: "Azure Application Insights Sink"
layout: default
---

# Azure Application Insights Sink

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
```

## What is it?

The Azure Application Insights sink is an extension of the [official Application Insights sink](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/) that allows you to not only emit traces or events, but the whole Application Insights suite of telemetry types - Traces, Dependencies, Events, Requests & Metrics.

You can easily configure the sink by providing the Azure Application Insights key:

```csharp
ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsights("<key>")
    .CreateLogger();
```

Alternatively, you can override the default minimum log level to reduce amount of telemetry being tracked :

```csharp
ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsights("<key>", restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();
```

## FAQ

### Q: Why is it mandatory to provide an instrumentation key?

While the native Azure Application Insights SDK does not enforce an instrumentation key we have chosen to make it mandatory to provide one.

By doing this, we allow you to fail fast and avoid running your application with a misconfigured telemetry setup. If it would be optional, you could have it running for days/weeks only to notice you are not sending any telemetry when everything is on fire and you are in the dark.

If you want to optionally use our sink when there is an instrumentation key, we recommend using this simple pattern:

```csharp
var loggerConfig =  new LoggerConfiguration()
    .MinimumLevel.Debug();

if(string.IsNullOrEmpty(key) == false)
{
    loggerConfig.WriteTo.AzureApplicationInsights(key);
}

ILogger logger = loggerConfig.CreateLogger();
```

[&larr; back](/)
