---
title: "Using Arcus & Serilog in .NET Core and/or Azure Functions"
layout: default
---

# Using Arcus & Serilog in .NET Core and/or Azure Functions

When using Arcus Observability & Serilog it is mandatory that your application is configured correctly to ensure everything works smoothly and all features are working fine.

We encourage you to follow the standard [Serilog instructions](https://github.com/serilog/serilog-aspnetcore#instructions) on setting your application up. 

Some aspects we would like to highlight are:

- Make sure to call [`UseSerilog`](https://www.nuget.org/packages/Serilog.AspNetCore) when creating a `IHostBuilder`
- Remove default for logging including its configuration in `appsettings.json` *(if applicable)*

If you cannot use `UseSerilog`, you can still configure it by using `AddSerilog` as a logging provider; but we recommend removing all other providers with `loggingBuilder.ClearProviders()` so that they don't interfer.

## Setting up Serilog with Azure Functions

Using Serilog with Azure Functions requires some guidance and we've made it a bit easier to use.

Before we get started, install our NuGet package for Azure Functions:

```
PM > Install-Package -Name Arcus.Observability.Telemetry.AzureFunctions
```

Once that is done, you can configure Serilog during startup as following:

```csharp
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Arcus.Samples.AzureFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName("Docker Hub Metrics Scraper")
                .Enrich.WithVersion()
                .WriteTo.Console()
                .WriteTo.AzureApplicationInsights(instrumentationKey)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProvidersExceptFunctionProviders();
                loggingBuilder.AddSerilog(logger);
            });
        }
    }
}
```

> :bulb: Note that we are using `ClearProvidersExceptFunctionProviders` instead of `ClearProviders` given Azure Functions requires some logging providers to be available.

Here is an example of how you can use ILogger to write multi-dimensional metrics with Arcus. If Serilog would not be setup correctly (see above), it would only report the metric without the dimensions.

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

        _logger.LogMetric("Image Pulls", pullCount, contextualInformation);
    }
}
```

