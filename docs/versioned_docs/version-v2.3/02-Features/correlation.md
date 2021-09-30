---
title: "Correlation"
layout: default
---

# Correlation

`CorrelationInfo` provides a common set of correlation levels:

- Transaction Id - ID that relates different requests together into a functional transaction.
- Operation Id - Unique ID information for a single request.
- Operation parent Id - ID of the original service that initiated the request.

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Correlation --version 2.3.0
```

## What We Provide

The `Arcus.Observability.Correlation` library provides a way to get access to correlation information across your application.
What it **DOES NOT** provide is how this correlation information is initially set.

It uses the the Microsoft dependency injection mechanism to register an `ICorrelationInfoAccessor` and `ICorrelationInfoAccessor<>` implementation that is available.

**Example**

```csharp
using Arcus.Observability.Correlation;

namespace Application
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds operation and transaction correlation to the application,
            // using the `DefaultCorrelationInfoAccessor` as `ICorrelationInfoAccessor` that stores the `CorrelationInfo` model internally.
            services.AddCorrelation();
        }
    }
}
```
## Custom Correlation

We register two interfaces during the registration of the correlation: `ICorrealtionInfoAccessor` and `ICorrelationInfoAccessor<>`.
The reason is because some applications require a custom `CorrelationInfo` model, and with using the generic interface `ICorrelationInfoAccessor<>` we can support this.

**Example**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Arcus.Observability.Correlation;

namespace Application
{
    public class OrderCorrelationInfo : CorrelationInfo
    {
        public string OrderId { get; }
    }

    public class Startup
    {
        public void ConfigureService(IServiceCollection services)
        {
            services.AddCorrelation<OrderCorrelationInfo>();
        }
    }
}
```

## Accessing Correlation Throughout the Application

When a part of the application needs access to the correlation information, you can inject one of the two interfaces:

```csharp
using Arcus.Observability.Correlation;

namespace Application
{
    public class OrderService
    {
        public OrderService(ICorrelationInfoAccessor accessor)
        {
             CorrelationInfo correlationInfo = accessor.CorrelationInfo;
        }
    }
}
```

Or, alternatively when using custom correlation:

```csharp
using Arcus.Observability.Correlation;

namespace Application
{
    public class OrderService
    {
        public OrderService(ICorrelationInfoAccessor<OrderCorrelationInfo> accessor)
        {
             OrderCorrelationInfo correlationInfo = accessor.CorrelationInfo;
        }
    }
}
```

## Configuration

The library also provides a way configure some correlation specific options that you can later retrieve during get/set of the correlation information in your application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCorrelation(options =>
    {
        // Configuration on the transaction ID (`X-Transaction-ID`) request/response header.
        // ---------------------------------------------------------------------------------

        // Whether the transaction ID can be specified in the request, and will be used throughout the request handling.
        // The request will return early when the `.AllowInRequest` is set to `false` and the request does contain the header (default: true).
        options.Transaction.AllowInRequest = true;

        // Whether or not the transaction ID should be generated when there isn't any transaction ID found in the request.
        // When the `.GenerateWhenNotSpecified` is set to `false` and the request doesn't contain the header, no value will be available for the transaction ID; 
        // otherwise a GUID will be generated (default: true).
        options.Transaction.GenerateWhenNotSpecified = true;

        // Whether to include the transaction ID in the response (default: true).
        options.Transaction.IncludeInResponse = true;

        // The header to look for in the request, and will be set in the response (default: X-Transaction-ID).
        options.Transaction.HeaderName = "X-Transaction-ID";

        // The function that will generate the transaction ID, when the `.GenerateWhenNotSpecified` is set to `false` and the request doesn't contain the header.
        // (default: new `Guid`).
        options.Transaction.GenerateId = () => $"Transaction-{Guid.NewGuid()}";

        // Configuration on the operation ID (`RequestId`) response header.
        // ----------------------------------------------------------------

        // Whether to include the operation ID in the response (default: true).
        options.Operation.IncludeInResponse = true;

        // The header that will contain the operation ID in the response (default: RequestId).
        options.Operation.HeaderName = "RequestId";

        // The function that will generate the operation ID header value.
        // (default: new `Guid`).
        options.Operation.GenerateId = () => $"Operation-{Guid.NewGuid()}";
    });
}
```

Later in the application, the options can be retrieved by injecting the `IOptions<CorrelationInfoOptions>` type.

### Custom Configuration

We also provide a way to provide custom configuration options when the application uses a custom correlation model.

For example, with a custom correlation model:

```csharp
using Arcus.Observability.Correlation;

namespace Application
{
    public class OrderCorrelationInfo : CorrelationInfo
    {
        public string OrderId { get; }
    }
}
```

We could introduce an `OrderCorrelationInfoOptions` model:

```csharp
using Arcus.Observability.Correlation;

namespace Application
{
    public class OrderCorrelationInfoOptions : CorrelationInfoOptions
    {
        public bool IncludeOrderId { get; set; }
    }
}
```

This custom options model can then be included when registering the correlation:

```csharp
using Microsoft.Excentions.DependencyInjection;

namespace Application
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelation<OrderCorrelationInfo, OrderCorrelationInfoOptions>(options => options.IncludeOrderId = true);
        }
    }
}
```


