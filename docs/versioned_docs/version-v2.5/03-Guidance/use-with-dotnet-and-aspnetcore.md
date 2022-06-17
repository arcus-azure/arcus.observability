---
title: "Using Arcus & Serilog in ASP.NET Core"
layout: default
---

# Using Arcus & Serilog in ASP.NET Core

When using Arcus Observability & Serilog it is mandatory that your application is configured correctly to ensure the telemetry is correctly send to Azure Application Insights.

## Setting up Serilog with ASP.NET Core

Using Serilog with ASP.NET Core requires you to install an ASP.NET Core specific Serilog package:

```shell
PM > Install-Package -Name Serilog.AspNetCore
```

When using Arcus.Observability, we expect you to setup Serilog as your logging infrastructure.
We encourage you to follow the standard [Serilog instructions](https://github.com/serilog/serilog-aspnetcore) on setting up Serilog for ASP.NET Core. Make sure to call [`UseSerilog`](https://www.nuget.org/packages/Serilog.AspNetCore).

Using Azure Application Insights requires you to install an Arcus specific package:

```shell
PM > Install-Package -Name Arcus.Observability.Telemetry.Serilog.Sinks.AzureApplicationInsights
```

Now you can configure Serilog and make sure that telemetry is written to Azure Application Insights using the components that are made available by Arcus Observability

We encourage you to use the [Azure Application Insights connection string](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string?tabs=net) to configure the Serilog sink: 

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.DependencyInjection.Extensions;
using Serilog;
using Serilog.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog(ConfigureLoggerConfiguration);
    }

    private static void ConfigureLoggerConfiguration(
        HostBuilderContext context,
        IServiceProvider serviceProvider,
        LoggerConfiguration config)
    {
        string connectionString = context.Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");

        config.ReadFrom.Configuration(context.Configuration)
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .Enrich.FromLogContext()
              .Enrich.WithVersion()
              .Enrich.WithComponentName("API")
              .Enrich.WithHttpCorrelationInfo(serviceProvider)
              .WriteTo.Console()
              .WriteTo.AzureApplicationInsightsWithConnectionString(connectionString);
    }
}
```

> 💡 Note that this setup is by default available in all the [Arcus project templates](https://templates.arcus-azure.net/).

## Writing telemetry with ILogger

If Serilog is correctly configured, you can write telemetry data with the general `ILogger` instance injected in your application. Without Arcus' configuration, you would only get the log message without the multi-dimensional telemetry information.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
public class OrderController : ControllerBase
{
    private readonly ILogger _logger;

    public OrderController(ILogger logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Post(Order order)
    {
        var contextualInformation = new Dictionary<string, object>
        {
            {"Order ID", order.Id},
            {"Customer", order.Customer.Name}
        };

        _logger.LogEvent("Order received", contextualInformation);
    }
}
```

See [our list of telemetry types](../02-Features/writing-different-telemetry-types.md) to find out all the available types that can be written with Arcus.