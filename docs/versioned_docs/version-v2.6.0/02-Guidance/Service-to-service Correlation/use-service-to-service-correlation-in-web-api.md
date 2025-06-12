---
title: "Use service-to-service correlation in Web API solutions"
layout: default
---

# Use service-to-service correlation in Web API solutions
The concept of service-to-service correlation is big and complex and spans multiple Arcus libraries. This user-guide will walk through all the available Arcus features that work together to facilitate service-to-service correlation in your Web API solutions. For a general introduction, see the [introduction page on service-to-service correlation](index.md).

To use service-to-service correlation in Web API solutions, both the sending and receiving side of the separate API components have to be adapted. The HTTP correlation will be passed through from one service to another via the HTTP headers which the internal Arcus system will use to link your services together.

The following diagram shows this communication more clearly:
![HTTP correlation diagram](/media/http-correlation.png)

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
5. Service B responds to service A with the same information as the call to service B.
6. The user receives both the **transaction ID** and **operation parent ID** in their final response.

> 💡 Additional configuration is available to tweak this functionality, see the [dedicated Arcus Web API feature documentation](https://webapi.arcus-azure.net/features/correlation) for more in-depth information on HTTP correlation.

## API Demonstration
In this user-guide, two fictive API applications will be used to represent Service A and Service B of the diagram. Both services will be adapted to use service-to-service correlation.

* **Product API** (Service A): receives an request to order a product if enough items of that product are in stock.
* **Stock API** (Service B): response with the available items in stock of a given product

The user interacts with the **Product API** to order their product. Internally, the **Product API** will contact the **Stock API** to verify if there are enough items of that product to complete the order. 

> ⚠ Take into account that this sample should only be used for demonstration purposes and does not reflect a fully production-ready implementation. We have chosen to only provide the bare bones of the application so the changes are clear.

### Product API: startup code
First, lets look at the initial code for the **Product API**. The startup code has the hosting and routing functionality to use API controllers. The configuration value `STOCK_API_URL` already reveals that this API will contact another API. This is also the reason why we added `.AddHttpClient()` to the startup code so we can inject `HttpClient` instances. ([More information on injecting HTTP clients in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0)). 
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRouting();
        builder.Services.AddControllers();
        
        builder.Configuration.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("STOCK_API_URL", "http://localhost:788")
        });
        builder.Services.AddHttpClient("Stock API");

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.Run("http://localhost:787");
    }
}
```

The sole API controller in the **Product API** makes sure that we receive the product order request, contact the **Stock API** to request the available stock for that product, and determine the user response based on that availability.
```csharp
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public class ProductOrderRequest
{
    public string ProductName { get; set; }
    public int Amount { get; set; }
}

[ApiController]
[Route("api/v1/product")]
public class ProductController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ProductController(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProductOrderRequest request)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("Stock API");
        
        var endpoint = $"{_configuration["STOCK_API_URL"]}/api/v1/stock?productName={orderRequest.ProductName}";

        var response = await httpClient.GetFromJsonAsync<ProductStockResponse>(endpoint);
        if (response.AvailableItems >= orderRequest.Amount)
        {
            return Accepted();
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
```

### Stock API: startup code
The startup code of the **Stock API** only has the hosting and routing functionality. Notice that this API is hosted on the same port that the **Product API** uses in its configuration.
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRouting();
        builder.Services.AddControllers();
       
        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.Run("http://localhost:788");
    }
}
```

The sole API controller in the **Stock API** returns a fictive amount of stock for a given product.
```csharp
using Microsoft.AspNetCore.Mvc;

public class ProductStockResponse
{
    public string ProductName { get; set; }
    public int AvailableItems { get; set; }
}

[ApiController]
[Route("/api/v1/stock")]
public class StockController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string productName)
    {
        var response = new ProductStockResponse
        {
            ProductName = productName,
            AvailableItems = 3
        };

        return Ok(response);
    }
}
```

Right now, we haven't used any Arcus functionality, only built-in ASP.NET Core features.

### Product API: add Arcus HTTP correlation
Now that we have explained the application code, we can add the Arcus functionality that will provide clear telemetry for the API interaction.

First, these packages need to be installed:
```shell
PM > Install-Package Arcus.WebApi.Logging -MinimumVersion 1.6.0
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights -MinimumVersion 2.5.0
```

> For more information on `Arcus.WebApi.Logging`, see [these dedicated feature docs](https://webapi.arcus-azure.net/features/logging), for more information on `Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights`, see [these dedicated feature docs](https://observability.arcus-azure.net/Features/sinks/azure-application-insights).

The following additions have to be made to the startup code for the **Product API** to be able to track the incoming/outgoing HTTP requests, and to log to Application Insights.
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRouting();
        builder.Services.AddControllers();
        
        // [Add] Makes HTTP correlation available throughout application
        builder.Services.AddHttpCorrelation();

        builder.Configuration.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("STOCK_API_URL", "http://localhost:788")
        });
        builder.Services.AddHttpClient("Stock API")
                        // [Add] Adds telemetry to outgoing request
                        .WithHttpCorrelationTracking();

        // [Add] Serilog configuration that writes to Application Insights
        builder.Host.UseSerilog((context, provider, config) =>
        {
            config.MinimumLevel.Verbose()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                  .Enrich.WithComponentName("Product API")
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
* `.WithHttpCorrelationTracking()`: enhances the injected `HttpClient`'s so that outgoing HTTP requests are enriched with the available application's HTTP correlation. This will also make sure that the send request will be tracked as a HTTP dependency ([more info](https://webapi.arcus-azure.net/features/correlation))
* `WriteTo.AzureApplicationInsightsWithConnectionString`: Serilog's sink that writes all the logged telemetry to Application Insights ([more info](https://observability.arcus-azure.net/Features/sinks/azure-application-insights))
* `app.UseHttpCorrelation()`: retrieves the HTTP correlation from the incoming request or generates a new set (first request). This correlation information is set into the `IHttpCorrelationInfoAccessor`. ([more info](https://webapi.arcus-azure.net/features/correlation)).
* `app.UseRequestTracking()`: tracks the incoming HTTP request as a telemetry request. ([more info](https://webapi.arcus-azure.net/features/logging)).

> ⚠ Note that when initializing the Application Insights Serilog sink, you should use the Arcus secret store to retrieve this connection string. Setting this up requires you to reload the logger after the application is build. For more information, see [this dedicated section](https://observability.arcus-azure.net/Features/sinks/azure-application-insights#q-where-can-i-initialize-the-logger-in-an-aspnet-core-application-or-other-hosted-service) that describes how to do this.

> ⚠ Note that the order of the middleware component registrations is important. The HTTP request tracking needs the endpoint routing to figure out if the request should be tracked, for example. For more information on our different middleware components, see [our Web API feature documentation](https://webapi.arcus-azure.net/features/logging).

### Stock API: add Arcus HTTP correlation
The **Stock API** will need similar changes, but different from the **Product API** is that this API doesn't have to send correlated requests, only receive them.

The same packages needs to be installed:
```shell
PM > Install-Package Arcus.WebApi.Logging -MinimumVersion 1.6.0
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights -MinimumVersion 2.5.0
```

The new startup code is very similar. The only difference is missing HTTP client registration:
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRouting();
        builder.Services.AddControllers();
        
        // [Add] Makes HTTP correlation available throughout application
        builder.Services.AddHttpCorrelation();

        // [Add] Serilog configuration that writes to Application Insights
        builder.Host.UseSerilog((context, provider, config) =>
        {
            config.MinimumLevel.Verbose()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                  .Enrich.WithComponentName("Stock API")
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
        app.Run("http://localhost:788");
    }
}
```

> ⚠ Note that when initializing the Application Insights Serilog sink, you should use the Arcus secret store to retrieve this connection string. Setting this up requires you to reload the logger after the application is build. For more information, see [this dedicated section](https://observability.arcus-azure.net/Features/sinks/azure-application-insights#q-where-can-i-initialize-the-logger-in-an-aspnet-core-application-or-other-hosted-service) that describes how to do this.

> ⚠ Note that the order of the middleware component registrations is important. The HTTP request tracking needs the endpoint routing to figure out if the request should be tracked, for example. For more information on our different middleware components, see [our Web API feature documentation](https://webapi.arcus-azure.net/features/logging).

### Run the API solution
When both services are adapted to use Arcus' HTTP correlation, we can run the solution. It's worth noting that during these changes, we didn't once change the application code (controllers); only the startup code. This one of the many benefits from Arcus as it abstracts away all the infrastructure and boilerplate code and doesn't pollute application code with something infrastructure-like as service-to-service correlation.

Once the applications run, we send a request to the **Product API**, like:
```powershell
curl -Method POST `
  -Headers @{'Content-Type'='application/json'}  `
  'http://localhost:787/api/v1/product' `
  -Body '{ "ProductName": "Fancy desk", "Amount": 3 }'

// StatusCode : 202
// Headers:   : X-Transaction-ID=8a622fa4-a757-47a0-9db9-e22d00328087
//              X-Operation-ID=05353d3b-de3f-44e6-90cb-9a622884f290
```

The real result, though, happens in Application Insights.

The application map (`Application Insights > Investigate > Application Map`) shows a clear relationship between the two services:
![Product API ↔️ Stock API application map example](/media/product-stock-api-service-correlation-example-applicationmap.png)

If you copy the `X-Transaction-ID` from the response (`8a622fa4-a757-47a0-9db9-e22d00328087`) and past it in the transaction search (`Application Insights > Investigate > Transaction search`), you'll see this relationship in more detail when you select the initial HTTP request to the **Product API**. You clearly see how the initial request to the **Product API** is the caller of the dependency towards the **Stock API**:
![Product API ↔️ Stock API transaction search example](/media/product-stock-api-service-correlation-example-transactionsearch.png)

## Conclusion
In this user guide, you've seen how the Arcus HTTP correlation functionality can be used to set up service-to-service correlation in Web API solutions. The service-to-service correlation is a very wide topic and can be configured with many options. See the [this documentation page](https://webapi.arcus-azure.net/features/correlation) to learn more about HTTP correlation in Web API applications.

## Further reading
* [Arcus Web API HTTP correlation documentation](https://webapi.arcus-azure.net/features/correlation)
* [Arcus Web API HTTP request tracking documentation](https://webapi.arcus-azure.net/features/logging)
* [Arcus Application Insights Serilog sink documentation](https://observability.arcus-azure.net/Features/sinks/azure-application-insights)
* Web API blogs
  * [Out-of-the-box Request Tracking, Simplified HTTP Correlation & JWT Authorization in Arcus Web API v1.0](https://www.codit.eu/blog/out-of-the-box-request-tracking-simplified-http-correlation-jwt-authorization-in-arcus-web-api-v1-0/)
  * [Enhanced request tracking in Arcus Web API v1.3](https://www.codit.eu/blog/enhanced-request-tracking-in-arcus-web-api-v1-3/)
* Observability blogs
  * [Announcing Arcus Observability](https://www.codit.eu/blog/announcing-arcus-observability/)
  * [Measure a Variety of Azure Dependencies with Observability v0.2](https://www.codit.eu/blog/measure-a-variety-of-azure-dependencies-with-observability-v0-2/)
  * [Service Correlation Preparation & .NET 6 Support in Arcus Observability v2.4](https://www.codit.eu/blog/service-correlation-preparation-net-6-support-in-arcus-observability-v2-4/)
  * [Service-to-Service Correlation, One of the Biggest Arcus Features in Observability v2.5](https://www.codit.eu/blog/service-to-service-correlation-one-of-the-biggest-arcus-features-in-observability-v2-5/)
