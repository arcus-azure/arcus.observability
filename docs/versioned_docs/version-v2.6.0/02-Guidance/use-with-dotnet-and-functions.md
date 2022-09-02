---
title: "Using Arcus & Serilog in Azure Functions (in-process)"
layout: default
---

# Using Arcus & Serilog in Azure Functions (in-process)
The Arcus Application Insights Serilog sink is a great way to simplify telemetry tracking and application logging. Unlike Microsoft's `TelemetryClient` for telemetry tracking, Arcus uses the common `ILogger` infrastructure to track and link telemetry in Application Insights.

This user guide will walk through the steps to configure the Arcus Application Insights Serilog sink in an Azure Functions (in-process) HTTP trigger application.

## Installation
The example in this user guide uses following packages. 

```shell
PM > Install-Package -Name Serilog.Extensions.Logging
PM > Install-Package -Name Serilog.Sinks.Console
PM > Install-Package -Name Arcus.Observability.Telemetry.AzureFunctions
PM > Install-Package -Name Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
PM > Install-Package -Name Arcus.WebApi.Logging.AzureFunctions
```

### Serilog.AspNetCore
Since Arcus uses Serilog, we expect you to setup Serilog as your logging infrastructure. This is also Microsoft's recommended logging system ([more info](https://serilog.net/)).

### Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights
The Arcus Application Insights Serilog sink builds on top of Serilog and acts as a bridge between the common `ILogger` infrastructure and the Application Insights telemetry.

### Arcus.WebApi.Logging
As this user guide shows how a fully working API application is set up for Application Insights tracking, we will also use `Arcus.WebApi.Logging`. The Arcus WebApi library builds on top of Arcus Observability, specific for API applications. This will make sure that the application correlation is stored in the HTTP context during send/receive operations ([more info](https://webapi.arcus-azure.net/)).

## Setting up Serilog with Arcus
The following shows a complete code sample of how Arcus and Serilog are configured together. Each critical point is explained afterwards. In short, what's happening is that (2) the Serilog configuration is built up to use Arcus Application Insights Serilog sink, (3) any default Application Insights interaction is removed, and (4) the Serilog configuration is registered in the application.

> ⚠ Make sure to remove the `Logging` section from the `appsettings.json` *(if applicable)* as this will not be used by Serilog

```csharp
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        // 1. Register the HTTP correlation services to access the current correlation information during the HTTP request processing.
        builder.AddHttpCorrelation();

        // 2. Create Serilog logger configuration model that writes to Application Insights.
        IConfiguration appConfig = builder.GetContext().Configuration;
        var connectionString = appConfig.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");
        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithComponentName("Azure HTTP Trigger")
            .Enrich.WithVersion()
            .WriteTo.Console()
            .WriteTo.AzureApplicationInsightsWithConnectionString(connectionString);

        builder.Services.AddLogging(logging =>
        {
            // 3. Remove Microsoft's default Application Insights logger provider.
            logging.RemoveMicrosoftApplicationInsightsLoggerProvider()
                    // 4. Use Serilog's logger configuration to create the application logger.
                   .AddSerilog(logConfig.CreateLogger(), dispose: true);
        });
    }
}
```

1. To make sure that any written telemetry to Application Insights is correlated, the Serilog sink should be able to access the current HTTP correlation of the received request. The `.AddHttpCorrelation()` makes sure that the HTTP correlation services are available.
2. Serilog configuration can be set up. This will extract the Application Insights connection string from the Arcus secret store and use it to configure the Arcus Serilog sink, using `AzureApplicationInsightsWithConnectionString`. This example doesn't have any HTTP correlation enrichment by-default, as the services container is not build at this moment. For correlated 
3. We need to call `RemoveMicrosoftApplicationInsightsLoggerProvider` to remove Microsoft's `ApplicationInsightsLoggerProvider` because it would conflict with our own Serilog Application Insights sink. We can't guarantee stable telemetry if Microsoft's logger provider is registered as this provider manipulates the telemetry before it get's send out to Application Insights. Removing it ensure that Arcus is in full control of the send-out telemetry.
4. Register the created Serilog configuration as the application logger.

> 💡 Note: this setup is by default available in the [Arcus Azure Functions HTTP trigger project template](https://templates.arcus-azure.net/features/azurefunctions-http-template).

## Writing telemetry with ILogger
Once Serilog is set up, you can write telemetry data with the general `ILogger` instance injected in your application. This example uses the Arcus logger extension to multi-dimensional metrics in Application Insights, but there is a lot more that can be tracked. See [our list of telemetry types](../03-Features/writing-different-telemetry-types.md) to find out all the available types that can be written with Arcus.

```csharp
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class DockerHubMetricScraperFunction
{
    private readonly DockerHubClient _dockerHubClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DockerHubMetricScraperFunction> _logger;

    public DockerHubMetricScraperFunction(DockerHubClient dockerHubClient, IConfiguration configuration, ILogger<DockerHubMetricScraperFunction> logger)
    {
        Guard.NotNull(dockerHubClient, nameof(dockerHubClient));
        Guard.NotNull(configuration, nameof(configuration));
        Guard.NotNull(logger, nameof(logger));

        _dockerHubClient = dockerHubClient;
        _configuration = configuration;
        _logger = logger;
    }

    [FunctionName("docker-hub-metric-scraper")]
    public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo timer)
    {
        _logger.LogInformation($"Starting to scrape Docker Hub metrics at {DateTime.UtcNow}");

        var repoName = _configuration["DOCKER_HUB_REPO_NAME"];
        var imageName = _configuration["DOCKER_HUB_IMAGE_NAME"];

        var pullCount = await _dockerHubClient.GetImageMetricsAsync(repoName, imageName);

        var contextualInformation = new Dictionary<string, object>
        {
            {"Repo Name", repoName},
            {"Image Name", imageName},
            {"Image ID", $"{repoName}/{imageName}"}
        };

        _logger.LogCustomMetric("Image Pulls", pullCount, contextualInformation);
    }
}
```