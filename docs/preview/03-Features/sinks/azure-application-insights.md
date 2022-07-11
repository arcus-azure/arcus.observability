---
title: "Azure Application Insights Sink"
layout: default
---

# Azure Application Insights Sink

## What is it?

The Azure Application Insights sink is an extension of the [official Application Insights sink](https://www.nuget.org/packages/Serilog.Sinks.ApplicationInsights/) that allows you to not only emit traces or events, but the whole Application Insights suite of telemetry types - Traces, Dependencies, Events, Requests & Metrics.

You can easily configure the sink by providing the Azure Application Insights connection string or instrumentation key, but the connection string is preferred.

```csharp
using Serilog;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>")
    .CreateLogger();

ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsightsWithInstrumentationKey("<key>")
    .CreateLogger();
```

Alternatively, you can override the default minimum log level to reduce amount of telemetry being tracked :

```csharp
using Serilog;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>", restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();
```

For more information on this sink: [see this section](#azure-application-insights-sink).

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
```

## Configuration

The Azure Application Insights sink has some additional configuration which can be changed to influence the tracking.

### Requests

### Request ID

When tracking requests, the ID for the request telemetry is by default a generated GUID. The generation of this ID can be configured via the options.
This is useful (for example) in a service-to-service correlation system where you want the ID of the incoming request to be based on the sending system, or you want to incorporate the operation ID in the request ID.

```csharp
using Serilog;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>", options =>
    {
        // Configurable generation function for the telemetry request ID.
        options.Request.GenerateId = () => $"my-custom-ID-{Guid.NewGuid()}";
    })
```

### Exceptions

#### Properties

When tracking exceptions, one can opt-in to track all the public properties of the exception which will be included as [custom dimensions](https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics#custom-measurements-and-properties-in-analytics).
These public properties are formatted with the following pattern: `Exception-{0}` where `{0}` is the place where the public property's name is inserted. 

The value of the property will be the value of the custom dimension so that the custom dimension will be in the form `"Exception-{your-property-name}" = "your-property-value"`.

This property format pattern can be configured, like shown in the following example:

```csharp
using Serilog;
using Serilog.Configuration;

ILogger logger = new LoggerConfiguration()
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>", options =>
    {
        // Opt-in to track all the first-level exception properties; inherited properties will not be included.
        options.Exception.IncludeProperties = true;

        // Property format to track the exception's properties (default: `"Exception-{0}"`)
        options.Exception.PropertyFormat = "CustomException.{0}");
    })
    .CreateLogger();
```

Let's take a custom exception to demonstrate:

```csharp
public class OrderingException : Exception
{
    ...

    // Property to be included in Application Insights.
    public int OrderNumber { get; }
}
```

With this configuration, the custom dimension name for the public properties of this exception will look like this: `"CustomException.OrderNumber"`.

> ⚠️ The property format should be in the correct form in order to be used. It's passed to the `String.Format` eventually which will try to insert the exception's property name.

### Correlation

When tracking telemetry, the correlation information is enriched via the [Arcus Serilog correlation enrichment](../telemetry-enrichment.md). 
This Serilog enricher is capable of customizing the log properties where the correlation will be added. When such custom setup is used, the same custom property names should be used here so that the Application Insights sink knows where to get the correlation information from.

```csharp
string operationIdPropertyName = "MyOperationPropertyId";
string transactionIdPropertyName = "MyTransactionPropertyId";
string operationParentPropertyName = "MyOperationParentPropertyId";
IServiceProvider serviceProvider = ...

ILogger logger = new LoggerConfiguration()
    .Enrich.WithCorrelationInfo(serviceProvider, options =>
    {
        options.Correlation.OperationIdPropertyName = operationIdPropertyName;
        options.Correlation.TransactionIdPropertyName = transactionIdPropertyName;
        options.Correlation.OperationParentIdPropertyName = operationParentIdPropertyName;
    })
    .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>", options =>
    {
        // Sets a custom property name where the operation ID should be added (default: 'OperationId').
        options.Correlation.OperationIdPropertyName = operationIdPropertyName;

        // Sets a custom property name where the transaction ID should be added (default: 'TransactionId');
        options.Correlation.TransactionIdPropertyName = transactionIdPropertyName;

        // Sets a custom property name where the operation parent ID should be added (default: 'OperationParentId').
        options.Correlation.OperationParentIdPropertyName = operationParentIdPropertyName;
    });
```

For more information on the way how Arcus handles correlation, see [this dedicated page](../correlation.md).

## FAQ

### Q: Why do I have to configure an instrumentation key (connection string) when using the Application Insights sink

While the native Azure Application Insights SDK does not enforce an instrumentation key we have chosen to make it mandatory to provide one.

By doing this, we allow you to fail fast and avoid running your application with a misconfigured telemetry setup. If it would be optional, you could have it running for days/weeks only to notice you are not sending any telemetry when everything is on fire and you are in the dark.

If you want to optionally use our sink when there is an instrumentation key, we recommend using this simple pattern:

```csharp
using Serilog;
using Serilog.Configuration;

var loggerConfig =  new LoggerConfiguration()
    .MinimumLevel.Debug();

if (!string.IsNullOrWhiteSpace(key))
{
    loggerConfig.WriteTo.AzureApplicationInsightsWithInstrumentationKey(key);
}

ILogger logger = loggerConfig.CreateLogger();
```

### Q: Where can I initialize the logger in an ASP.NET Core application or other hosted service?

Simply use the `UseSerilog` extension method on `IHostBuilder` which accepts an `Action<HostBuilderContext, IServiceProvider, LoggerConfiguration>`.
This Action gives you access to the configuration and the configured services.

If the connection string is stored as a secret in -for instance- Azure KeyVault, the `ISecretProvider` from [Arcus secret store](https://security.arcus-azure.net/features/secret-store) can be used to retrieve the connection string.

```csharp
using Serilog.Configuration;
using Arcus.Security.Core;

...
IHostBuilder host = 
    Host.CreateDefaultBuilder()
    .ConfigureSecretStore((context, config, builder) =>
    {
        // Configure the secret store here.
        // See: https://security.arcus-azure.net/features/secret-store/
    })
    .UseSerilog((context, serviceProvider, loggerConfig) =>
    {
         loggerConf.Enrich.FromLogContext()
                   .WriteTo.Console();

         var secretProvider = serviceProvider.GetService<ISecretProvider>();

         string connectionString = secretProvider.GetRawSecretAsync("ApplicationInsights:ConnectionString").GetAwaiter().GetResult();

         if (!string.IsNullOrWhiteSpace(connectionString))
         {
             loggerConfiguration.WriteTo.AzureApplicationInsightsWithConnectionString(connectionString, LogEventLevel.Information);
         }
    });
```