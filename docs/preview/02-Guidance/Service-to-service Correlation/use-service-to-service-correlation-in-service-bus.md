---
title: "Use service-to-service correlation in Azure Service Bus solutions"
layout: default
---

# Use service-to-service correlation in Azure Service Bus solutions
The concept of service-to-service correlation is big and complex and spans multiple Arcus libraries. This user-guide will walk through all the available Arcus features that work together to facilitate service-to-service correlation in your Azure Service Bus solutions. For a general introduction, see the [introduction page on service-to-service correlation](index.md).

To use service-to-service correlation in Azure Service Bus solutions, both the sending and receiving side of the separate components have to be adapted. The correlation will be passed through from one service to another via the Azure Service Bus message application properties which Arcus will use to link your services together.

The following diagram shows how a Web API calls a Service Bus with correlation tracking:
![Messaging correlation diagram](/media/service-to-service-api-worker-diagram-example.png)

## How does this work?
Three kind of correlation ID's are used to create the relationship:
* **Transaction ID**: this ID is the one constant in the diagram. This ID is used to describe the entire transaction, from beginning to end. All telemetry will be tracked under this ID.
* **Operation ID**: this ID describes a single operation within the transaction. This ID is used within a service to link all telemetry of that service together.
* **Operation Parent ID**: this ID is create the parent/child link across services. When service A calls service B, then service A is the so called 'parent' of service B.

The following list shows each step in the diagram:
1. The initial call in this example doesn't contain any correlation headers. This can be seen as a first interaction call to a service. 
2. Upon receiving at service A, the application will generate new correlation information. This correlation info will be used when telemetry is tracked on the service.
3. When a call is made to service B, the **transaction ID** is sent but also the **operation parent ID** in the form of a hierarchical structure.
4. The `jkl` part of this ID, describes the new parent ID for service B (when service B calls service C, then it will use `jkl` as parent ID)
5. The user receives both the **transaction ID** and **operation parent ID** in their final response.

> 💡 Additional configuration is available to tweak this functionality, see the [dedicated Arcus Messaging feature documentation](https://messaging.arcus-azure.net/Features/message-pumps/service-bus) for more in-depth information on Azure Service Bus correlation.

## Service Bus demonstration
In this user-guide, a fictive API and Service Bus application will be used to represent Service A and Service B of the diagram. Both services will be adapted to use service-to-service correlation.

* **Order API** (Service A): receives an request to order a product, sends the order to process.
* **Order Worker** (Service B): receives the order request and processes the order.

The user interacts with the **Product API** to order their product. Internally, the **Order API** will contact the **Order Worker** to further process the order request.

> ⚠ Take into account that this sample should only be used for demonstration purposes and does not reflect a fully production-ready implementation. We have chosen to only provide the bare bones of the application to be able to focus on the changes required for service-to-service correlation.

### Order API: startup code
First, lets look at the initial code for the **Order API**. The startup code has the hosting and routing functionality to use API controllers.
The `Azure.Messaging.ServiceBus` package is added so that the `AddServiceBusClientWithNamespace` becomes available. This will inject a `ServiceBusClient` in the application which will contact the **Order Worker**. 
```csharp
using Microsoft.Extensions.Azure;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRouting();
        builder.Services.AddControllers();

        builder.Services.AddAzureClients(clients =>
        {
          clients.AddServiceBusClientWithNamespace("<fully-qualified-servicebus-namespace>")
                 .WithName("Order Worker")
                 .WithCredential(new ManagedIdentityCredential());
        });

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.Run("http://localhost:787");
    }
}
```

The sole API controller in the **Order API** makes sure that we receive the product order request and contact the **Order Worker** to further process the order request.
```csharp
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

[ApiController]
[Route("api/v1/order")]
public class OrderController : ControllerBase
{
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IAzureClientFactory<ServiceBusClient> clientFactory,
        ILogger<OrderController> logger)
    {
         ServiceBusClient client = clientFactory.CreateClient("Order Worker");
        _serviceBusSender = client.CreateSender("orders");
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProductOrderRequest productRequest)
    {
        var order = new Order(productRequest);
        var message = new ServiceBusMessage(order);

        await _serviceBusSender.SendMessageAsync(message);
    }
}
```

### Order Worker: startup code
The **Order Worker** application will use mostly Arcus functionality, so for now, the startup code is only creating the hosting functionality:
```csharp
using Microsoft.Extensions.Azure;

public class Program
{
    public static void Main(string[] args)
    {
       Host.CreateDefaultBuilder(args)
           .Build()
           .Run();
    }
}
```

Right now, we haven't used any Arcus functionality, only common Microsoft-available features.

### Order API: add Arcus correlation
Now that we have explained the application code, we can add the Arcus functionality that will provide clear telemetry for the API interaction.

First, these packages need to be installed:
```shell
PM > Install-Package Arcus.WebApi.Logging -MinimumVersion 1.6.1
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights -MinimumVersion 2.6.0
PM > Install-Package Azure.Messaging.ServiceBus -MinimumVersion 7.11.1
```

> ⚡ Note that these Arcus packages and additions are built-in into the [Arcus project templates](https://templates.arcus-azure.net/).

> For more information on `Arcus.WebApi.Logging`, see [these dedicated feature docs](https://webapi.arcus-azure.net/features/logging), for more information on `Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights`, see [these dedicated feature docs](https://observability.arcus-azure.net/Features/sinks/azure-application-insights).

The following additions have to be made to the startup code for the **Product API** to be able to track the incoming/outgoing HTTP requests, and to log to Application Insights.
```csharp
using Microsoft.Extensions.Azure;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRouting();
        builder.Services.AddControllers();

        // [Add] Makes HTTP correlation available throughout application
        builder.Services.AddHttpCorrelation();

        builder.Services.AddAzureClients(clients =>
        {
          clients.AddServiceBusClientWithNamespace("<fully-qualified-servicebus-namespace>")
                 .WithName("Order Worker")
                 .WithCredential(new ManagedIdentityCredential());
        });

        // [Add] Arcus + Microsoft component name/version registration
        builder.Services.AddAppName("Order API");
        builder.Services.AddAssemblyAppVersion<Program>();

        // [Add] Serilog configuration that writes to Application Insights
        builder.Host.UseSerilog((context, provider, config) =>
        {
            config.MinimumLevel.Information()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                  .Enrich.WithComponentName(provider)
                  .Enrich.WithVersion(provider)
                  .Enrich.WithHttpCorrelationInfo(provider)
                  .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>");
        });

        WebApplication app = builder.Build();
        // [Add] Gets HTTP correlation from incoming request
        app.UseHttpCorrelation();
        app.UseRouting();
        // [Add] Tracks incoming request
        app.UseRequestTracking();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.Run("http://localhost:787");
    }
}
```

Following additions are made:
* `builder.Services.AddHttpCorrelation()`: adds the HTTP correlation services to the application services. This registers the `IHttpCorrelationInfoAccessor` that is used to get/set the HTTP correlation throughout the application. ([more info](https://webapi.arcus-azure.net/features/correlation))
* `AddAppName`/`Add...AppVersion`: configures for both Arcus & Microsoft the component's name and version when tracking telemetry via either Arcus technology or directly via Microsoft's `TelemetryClient` ([more info](https://observability.arcus-azure.net/Features/telemetry-enrichment)).
* `WriteTo.AzureApplicationInsightsWithConnectionString`: Serilog's sink that writes all the logged telemetry to Application Insights ([more info](https://observability.arcus-azure.net/Features/sinks/azure-application-insights))
* `app.UseHttpCorrelation()`: retrieves the HTTP correlation from the incoming request or generates a new set (first request). This correlation information is set into the `IHttpCorrelationInfoAccessor`. ([more info](https://webapi.arcus-azure.net/features/correlation)).
* `app.UseRequestTracking()`: tracks the incoming HTTP request as a telemetry request. ([more info](https://webapi.arcus-azure.net/features/logging)).

> ⚠ Note that when initializing the Application Insights Serilog sink, you should use the Arcus secret store to retrieve this connection string. Setting this up requires you to reload the logger after the application is build. For more information, see [this dedicated section](https://observability.arcus-azure.net/Features/sinks/azure-application-insights#q-where-can-i-initialize-the-logger-in-an-aspnet-core-application-or-other-hosted-service) that describes how to do this.

> ⚠ Note that the order of the middleware component registrations is important. The HTTP request tracking needs the endpoint routing to figure out if the request should be tracked, for example. For more information on our different middleware components, see [our Web API feature documentation](https://webapi.arcus-azure.net/features/logging).

With these HTTP correlation additions, we can easily put a message on the Service Bus queue, without any additional Arcus-related functionality:
```csharp
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

[ApiController]
[Route("api/v1/order")]
public class OrderController : ControllerBase
{
    private readonly ServiceBusSender _serviceBusSender;

    public OrderController(IAzureClientFactory<ServiceBusClient> clientFactory)
    {
         ServiceBusClient client = clientFactory.CreateClient("Order Worker");
        _serviceBusSender = client.CreateSender("orders");
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProductOrderRequest productRequest)
    {
        var order = new Order(productRequest);

        var data = BinaryData.FromObjectAsJson(order);
        await _serviceBusSender.SendMessageAsync(data);

        return Accepted();
    }
}
```

### Order Worker: add Arcus functionality
The **Order Worker** didn't had any implementation, so let's add this now. We want to receive a message on an Azure Service Bus queue, and track that as a linked request in Application Insights.
We will be using the [Arcus message pump](https://messaging.arcus-azure.net/Features/message-pumps/service-bus) for this, as it allows us to focus solely on the message handling and does request tracking built-in.

First, let's install these packages:
```shell
PM > Install-Package Arcus.Messaging.Pumps.ServiceBus -MinimumVersion 1.4.0
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights -MinimumVersion 2.7.0
PM > Install-Package Serilog.Extensions.Hosting
```

> ⚡ Note that these additions are built-in into the [Arcus project templates](https://templates.arcus-azure.net/).

These packages allows us to register the message pump and the Serilog logging:
```csharp
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

public class Program
{
    public static void Main(string[] args)
    {
        IHost host =
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    // [Add] Arcus + Microsoft component name/version registration
                    services.AddAppName("Order Worker");
                    services.AddAssemblyAppVersion<Program>();

                    // [Add] Arcus message pump on Service Bus 'orders' queue
                    services.AddServiceBusQueueMessagePumpUsingManagedIdentity("orders", "<fully-qualified-servicebus-namespace>")
                            // [Add] Arcus message handler that processes the received 'order' on the Service Bus queue
                            .WithServiceBusMessageHandler<OrderMessageHandler, Order>();
               })
               // [Add] Register the Serilog logger as the application's logger
               .UseSerilog(Log.Logger)
               .Build();

        // [Add] Serilog configuration that writes to Application Insights
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithVersion(host.Services)
            .Enrich.WithComponentName(host.Services)
            .WriteTo.AzureApplicationInsightsWithConnectionString("<connection-string>")
            .CreateLogger();

        host.Run();
    }
}
```

Following additions are made:
* `AddAppName`/`Add...AppVersion`: configures for both Arcus & Microsoft the component's name and version when tracking telemetry via either Arcus technology or directly via Microsoft's `TelemetryClient` ([more info](https://observability.arcus-azure.net/Features/telemetry-enrichment)).
* `AddServiceBusQueueMessagePumpUsingManagedIdentity`: registers an Arcus message pump listening on an Azure Service Bus 'orders' queue. Received messages will automatically be tracked as Service Bus requests in Application Insights.
  * `WithServiceBusMessageHandler`: registers a custom `OrderMessageHandler` to process the deserialized `Order` message.
* `WriteTo.AzureApplicationInsightsWithConnectionString`: Serilog's sink that writes all the logged telemetry to Application Insights ([more info](https://observability.arcus-azure.net/Features/sinks/azure-application-insights))

> ⚠ Note that when initializing the Application Insights Serilog sink, you should use the Arcus secret store to retrieve this connection string. Setting this up requires you to reload the logger after the application is build. For more information, see [this dedicated section](https://observability.arcus-azure.net/Features/sinks/azure-application-insights#q-where-can-i-initialize-the-logger-in-an-aspnet-core-application-or-other-hosted-service) that describes how to do this.

The `OrderMessageHandler` is currently doing nothing:
```csharp
using Arcus.Messaging.Abstractions;
using Arcus.Messaging.Abstractions.ServiceBus;
using Arcus.Messaging.Abstractions.ServiceBus.MessageHandling;
using Microsoft.Extensions.Logging;

public class OrderMessageHandler : IAzureServiceBusMessageHandler<Order>
{
    private readonly ILogger<OrderMessageHandler> _logger;

    public OrderMessageHandler(ILogger<OrderMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task ProcessMessageAsync(
        Order message,
        AzureServiceBusMessageContext messageContext,
        MessageCorrelationInfo correlationInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order processed!");
    }
}
```

> 💡 For more information on how Arcus handles message handling in message pumps, see [our dedicated messaging feature documentation](https://messaging.arcus-azure.net/Features/message-pumps/service-bus).

### Run the solution
When both services are adapted, we can run the solution. Arcus makes sure that the link between the API and Service Bus is visible in Application Insights, without much effort from the consumer. Note that the `OrderMessageHandler` also receives a correlation model in its `ProcessMessageAsync` method which allows users to interact further with the passed-along correlation.

Once the applications run, we send a request to the **Order API**, like:
```powershell
curl -Method POST `
   -Headers @{'Content-Type'='application/json'}  `
   'http://localhost:5000/api/v1/order' `
   -Body '{ "ProductName": "Fancy desk", "Amount": 3 }'

// StatusCode : 202
// Headers:   : X-Transaction-ID=9d02c0f4782b45618181e84c4221b056
//              X-Operation-ID=f63bcab4b52b8373
```

The real result, though, happens in Application Insights.

The application map (`Application Insights > Investigate > Application Map`) shows a clear relationship between the two services:
![Product API <> Stock API application map example](/media/servicebus-worker-service-w3c-correlation-example-applicationmap.png)

If you copy the `X-Transaction-ID` from the response (`585ea273-eaa9-44bb-905a-6805bd418566`) and past it in the transaction search (`Application Insights > Investigate > Transaction search`), you'll see this relationship in more detail when you select the initial HTTP request to the **Order API**. You clearly see how the initial request to the **Order API** is the caller of the dependency towards the **Order Worker**:
![Product API <> Stock API transaction search example](/media/servicebus-worker-service-w3c-correlation-example-transactionsearch.png)

## Further reading
* [Arcus Service Bus messaging documentation](https://messaging.arcus-azure.net/Features/message-pumps/service-bus)
* [Arcus Application Insights Serilog sink documentation](https://observability.arcus-azure.net/Features/sinks/azure-application-insights)
* Messaging blogs
  * [Taking messaging to the next level with Arcus Messaging v1.0](https://www.codit.eu/blog/taking-messaging-to-the-next-level-with-arcus-messaging-v1-0/)
* Web API blogs
  * [Out-of-the-box Request Tracking, Simplified HTTP Correlation & JWT Authorization in Arcus Web API v1.0](https://www.codit.eu/blog/out-of-the-box-request-tracking-simplified-http-correlation-jwt-authorization-in-arcus-web-api-v1-0/)
  * [Enhanced request tracking in Arcus Web API v1.3](https://www.codit.eu/blog/enhanced-request-tracking-in-arcus-web-api-v1-3/)
* Observability blogs
  * [Announcing Arcus Observability](https://www.codit.eu/blog/announcing-arcus-observability/)
  * [Measure a Variety of Azure Dependencies with Observability v0.2](https://www.codit.eu/blog/measure-a-variety-of-azure-dependencies-with-observability-v0-2/)
  * [Service Correlation Preparation & .NET 6 Support in Arcus Observability v2.4](https://www.codit.eu/blog/service-correlation-preparation-net-6-support-in-arcus-observability-v2-4/)