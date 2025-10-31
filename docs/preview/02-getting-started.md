---
sidebar_label: Getting started
---

# Getting started with Arcus Observability
**Welcome to Arcus Observability!** 🎉

This page is dedicated to be used as a walkthrough on how to set up Arcus Observability in a new or existing project. Arcus Observability is an umbrella term for a set of NuGet packages that kickstart your observability solution.

## The basics
Arcus Observability is split between two major observability backend implementations: OpenTelemetry and Serilog. This split gets translated in two different packages:
* `Arcus.Observability.Telemetry.OpenTelemetry`
* `Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights`

The difference being that the Serilog-variant already assumes that the observability backend is [Azure Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview), while the OpenTelemetry-variant is more flexible in its observability backend.

Arcus Observability makes a distinction in logging and telemetry, where telemetry is both traces (meaning: service-to-service traces in a trace context, not to be confused with 'logging traces') and metrics. Arcus allows that telemetry gets tracked with a single, central point of contact: the `IObservability` interface facade. The following sections will explain how to introduce this interface in your application.

## Track telemetry in an application using OpenTelemetry
Assuming that the application is currently set up to use OpenTelemetry to track its telemetry, the following Arcus package needs to be installed before continuing:
```powershell
PS> Install-Package -Name Arcus.Observability.Telemetry.OpenTelemetry
```

Registering Arcus Observability facade in your OpenTelemetry setup is as much as adding a single line:
```diff
using OpenTelemetry;

Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddOpenTelemetry()
+               .UseObservability();
    });
```

:::note
Internally, the magic happens where Arcus' `IObservability` point of contact gets registered and provides a way to handle .NET `Activity` and `Meter` instances.
:::

Tracking telemetry is now available in your application via the `IObservability` facade that can be injected in your services that are not yet automatically tracked by either OpenTelemetry or Microsoft.
```csharp
public class MongoDbUserRepository(IMongoCollection<User> collection, IObservability observability)
{
    public async Task<StorageResult> DeleteUserByIdAsync(string userId)
    {
        var telemetryContext = new Dictionary<string, object>
        {
            ["Collection"] = "Users",
            ["UserId"] = userId
        };
        // highlight-next-line
        using (var dependency = observability.StartCustomDependency("Delete User", telemetryContext))
        {
            try
            {
                // TODO: interact with Mongo DB.
            }
            catch (MongoException exception)
            {
                // highlight-next-line
                dependency.IsSuccessful = false;
            }
        }
    }
}
```

For more information on the possibilities of the `IObservability` facade, see the dedicated [feature documentation page](./03-Features/01-Telemetry/01-observability-facade.mdx).