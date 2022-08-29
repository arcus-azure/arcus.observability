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
PM > Install-Package Arcus.Observability.Correlation
```

## What We Provide

The `Arcus.Observability.Correlation` library provides a way to get access to correlation information across your application.
What it **DOES NOT** provide is how this correlation information is initially set.

It uses the Microsoft dependency injection mechanism to register an `ICorrelationInfoAccessor` and `ICorrelationInfoAccessor<>` implementation that is available.

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

We register two interfaces during the registration of the correlation: `ICorrelationInfoAccessor` and `ICorrelationInfoAccessor<>`.
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