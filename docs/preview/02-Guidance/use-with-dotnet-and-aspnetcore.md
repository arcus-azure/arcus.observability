---
title: "Using Arcus & Serilog in ASP.NET Core"
layout: default
---

# Using Arcus & Serilog in ASP.NET Core
The Arcus Application Insights Serilog sink is a great way to simplify telemetry tracking and application logging. Unlike Microsoft's `TelemetryClient` for telemetry tracking, Arcus uses the common `ILogger` infrastructure to track and link telemetry in Application Insights.

This user guide will walk through the steps to configure the Arcus Application Insights Serilog sink in ASP.NET Core applications.

## Installation
The example in this user guide uses following packages. 

```shell
PM > Install-Package -Name Serilog.AspNetCore
PM > Install-Package -Name Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
PM > Install-Package -Name Arcus.Security.Providers.AzureKeyVault
PM > Install-Package -Name Arcus.WebApi.Logging
```

### Serilog.AspNetCore
Since Arcus uses Serilog, we expect you to setup Serilog as your logging infrastructure. This is also Microsoft's recommended logging system ([more info](https://serilog.net/)).

### Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
The Arcus Application Insights Serilog sink builds on top of Serilog and acts as a bridge between the common `ILogger` infrastructure and the Application Insights telemetry.

### Arcus.Security.Providers.AzureKeyVault
It is recommended to configure the Arcus Application Insights Serilog sink with the [Application Insights connection string](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string). This connection string should be safely stored. This example uses the Arcus secret store with Azure Key Vault integration ([more info](https://security.arcus-azure.net/features/secret-store)). 

### Arcus.WebApi.Logging
As this user guide shows how a fully working API application is set up for Application Insights tracking, we will also use `Arcus.WebApi.Logging`. The Arcus WebApi library builds on top of Arcus Observability, specific for API applications. This will make sure that the application correlation is stored in the HTTP context during send/receive operations ([more info](https://webapi.arcus-azure.net/)).

## Setting up Serilog with Arcus
The following shows a complete code sample of how Arcus and Serilog are configured together. Each critical point is explained afterwards. In short, what's happening is that Serilog is first configured as a basic debug logger so that startup failures are logged to the console (1). After the application is build, the 'real' Serilog setup is configured that uses the Arcus Application Insights Serilog sink (4) with the connection string that is stored in the Arcus secret store (3).

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.DependencyInjection.Extensions;
using Serilog;
using Serilog.Configuration;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 1. Configure startup Serilog logger.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRouting();
            builder.Services.AddControllers();

            // 2. Configure Arcus secret store with Application Insights connection string (more info on secret store: https://security.arcus-azure.net/features/secret-store).
            builder.Host.ConfigureSecretStore((context, stores) => stores.AddAzureKeyVaultWithManagedServiceIdentity(...));
            
            // 3. Use Serilog's static Logger as application logger.
            builder.Host.UserSerilog(Log.Logger);

            WebApplication app = builder.Build();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            
            await ConfigureSerilogAsync(app);
            await app.RunAsync("http://localhost:5000");

            return 0;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // 4. Retrieve Application Insights connection string to configure the 'real' application logger.
    private static async Task ConfigureSerilogAsync(WebApplication app)
    {
        var secretProvider = app.Services.GetRequiredService<ISecretProvider>();
        string connectionString = await secretProvider.GetRawSecretAsync("APPLICATIONINSIGHTS_CONNECTION_STRING");
            
        var reloadLogger = (ReloadableLogger) Log.Logger;
        reloadLogger.Reload(config =>
        {
            config.ReadFrom.Configuration(app.Configuration)
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                  .Enrich.FromLogContext()
                  .Enrich.WithVersion()
                  .Enrich.WithComponentName("API")
                  .Enrich.WithHttpCorrelationInfo(app.Services)
                  .WriteTo.Console()
                  .WriteTo.AzureApplicationInsightsWithConnectionString(connectionString);
            
            return config;
        });
    }
}
```

1. Before the application is build, we should set up a basic Serilog logger to log any startup failures. That's what the `try/catch/finally` block is all about. This is a safe and reliable way to setup Serilog ([more info](https://github.com/serilog/serilog-aspnetcore)). Note that the `.CreateBootstrapLogger()` means that the Serilog logger can be 'reloaded' afterwards.
2. The connection string to contact the Application Insights resource should be stored safely. The `.ConfigureSecretStore` will register a composite `ISecretProvider` interface to interact with all the registered secret providers ([more info](https://security.arcus-azure.net/features/secret-store)).
3. Make sure that Serilog uses the static configured logger as application logger that gets injected as `ILogger` in your application ([more info](https://github.com/serilog/serilog-aspnetcore)).
4. When the application is build, the 'real' Serilog configuration can be set up. This will extract the Application Insights connection string from the Arcus secret store and use it to configure the Arcus Serilog sink, using `AzureApplicationInsightsWithConnectionString`. This example also adds `WithHttpCorrelationInfo` from the `Arcus.WebApi.Logging` library to include the HTTP correlation ([more info](https://webapi.arcus-azure.net/features/correlation)).


> 💡 Note: this 4-step setup is by default available in the [Arcus web API template](https://templates.arcus-azure.net/features/web-api-template).

## Writing telemetry with ILogger
Once Serilog is set up, you can write telemetry data with the general `ILogger` instance injected in your application. This example uses the Arcus logger extension to track custom events in Application Insights, but there is a lot more that can be tracked. See [our list of telemetry types](../03-Features/writing-different-telemetry-types.md) to find out all the available types that can be written with Arcus.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/v1/order")]
public class OrderController : ControllerBase
{
    private readonly ILogger _logger;

    public OrderController(ILogger <OrderController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Post([FromBody] Order order)
    {
        // Use logger for general logging: results in 'trace' in Application Insights.
        _logger.LogInformation("Order {Id} processed!", order.Id);

        var contextualInformation = new Dictionary<string, object>
        {
            {"Order ID", order.Id},
            {"Customer", order.Customer.Name}
        };

        // Use logger for telemetry tracking: results in 'custom event' in Application Insights.
        _logger.LogCustomEvent("Order processed", contextualInformation);

        return Accepted();
    }
}
```