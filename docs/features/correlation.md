---
title: "Correlation"
layout: default
---

# Correlation

`CorrelationInfo` provides a common set of correlation levels:

- Transaction Id - ID that relates different requests together into a functional transaction.
- Operation Id - Unique ID information for a single request.

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Correlation
```

## What We Provide

The `Arcus.Observability.Correlation` library provides a way to get access to correlation information across your application.
What it **DOES NOT** provide is how this correlation information is initially set.

It uses the the Microsoft dependency injection mechanism to register an `ICorrelationInfoAccessor` implementation that is available.

**Example**

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Adds operation and transaction correlation to the application,
        // using the `DefaultCorrelationInfoAccessor` as `ICorrelationInfoAccessor` that stores the `CorrelationInfo` model internally.
        services.AddCorrelation();
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

[&larr; back](/)
=======
[&larr; back](/)
>>>>>>> master
