---
title: "Telemetry Enrichment"
layout: default
---

# Telemetry Enrichment

We provide a variety of enrichers for Serilog:

- [Application Enricher](#application-enricher)
- [Correlation Enricher](#correlation-enricher)
- [Kubernetes Enricher](#kubernetes-enricher)
- [Version Enricher](#version-enricher)

## Installation

This feature requires to install our NuGet package

```shell
PM > Install-Package Arcus.Observability.Telemetry.Serilog.Enrichers
```

## Application Enricher

The `Arcus.Observability.Telemetry.Serilog.Enrichers` library provides a [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment)
that adds the application's component name to the log event as a log property with the name `ComponentName` and gives the opportiunity to choose the location from where the application 'instance' should be retrieved.

**Example**
Name: `ComponentName`
Value: `My application component`

**Usage**

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithComponentName("My application component")
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {ComponentName: My application component, MachineName: MyComputer}
```

Or, alternatively one can choose to use the Kubernetes information which our [Application Insights](./sinks/azure-application-insights) sink will prioritize above the `MachineName` when determining the telemetry `Cloud.RoleInstance`.

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithComponentName("My application component")
    .Enrich.WithKubernetesInfo()
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {ComponentName: My application component, MachineName: MachineName: MyComputer, PodName: demo-app}
```

### Custom Serilog property names

The application enricher allows you to specify the name of the log property that will be added to the log event during enrichment.
By default this is set to `ComponentName`.

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithComponentName(
        componentName: "My application component", 
        propertyName: "MyComponentName")
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {MyComponentName: My application component, MachineName: MyComputer}
```

## Correlation Enricher

The `Arcus.ObservabilityTelemetry.Serilog.Enrichers` library provides a [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment)
that adds the `CorrelationInfo` information from the current context as log properties with the names `OperationId` and `TransactionId`.

You can use your own `ICorrelationInfoAccessor` implementation to retrieve this `CorrelationInfo` model,
or use the default `DefaultCorrelationInfoAccessor` implementation that stores this model

**Example**
Name: `OperationId`
Value: `52EE2C00-53EE-476E-9DAB-C1234EB4AD0B`

Name: `TransactionId`
Value: `0477E377-414D-47CD-8756-BCBE3DBE3ACB`

**Usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Serilog;

IServiceProvider serviceProvider = ...
ILogger logger = new LoggerConfiguration()
    .Enrich.WithCorrelationInfo(serviceProvider)
    .CreateLogger();

logger.Information("This event will be enriched with the correlation information");
```

Or alternatively, with a custom `ICorrelationInfoAccessor`:

```csharp
using Arcus.Observability.Colleration;
using Serilog;

ICorrelationInfoAccessor myCustomAccessor = ...

ILogger logger = new LoggerConfiguration()
    .Enrich.WithCorrelationInfo(myCustomAccessor)
    .CreateLogger();

logger.Information("This event will be enriched with the correlation information");
// Output: This event will be enriched with the correlation information {OperationId: 52EE2C00-53EE-476E-9DAB-C1234EB4AD0B, TransactionId: 0477E377-414D-47CD-8756-BCBE3DBE3ACB}
```

### Custom Serilog property names

The correlation information enricher allows you to specify the names of the log properties that will be added to the log event during enrichment.
This is available on all extension overloads. By default the operation ID is set to `OperationId` and the transaction ID to `TransactionId`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Serilog;

IServiceProvider serviceProvider = ...
ILogger logger = new LoggerConfiguration()
    .Enrich.WithCorrelationInfo(
        serviceProvider,
        operationIdPropertyName: "MyOperationId",
        transactionIdPropertyName: "MyTransactionId")
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {MyOperationId: 52EE2C00-53EE-476E-9DAB-C1234EB4AD0B, MyTransactionId: 0477E377-414D-47CD-8756-BCBE3DBE3ACB}
```

## Kubernetes Enricher

The `Arcus.Observability.Telemetry.Serilog.Enrichers` library provides a [Kubernetes](https://kubernetes.io/) [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment) 
that adds several machine information from the environment (variables).

**Example**

| Environment Variable   | Log Property |
| ---------------------- | ------------ |
| `KUBERNETES_NODE_NAME` | NodeName     |
| `KUBERNETES_POD_NAME`  | PodName      |
| `KUBERNETES_NAMESPACE` | Namespace    |

**Usage**

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithKubernetesInfo()
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {NodeName: demo-cluster, PodName: demo-app, Namespace: demo}
```

Here is an example of a Kubernetes YAML that provides the required environment variables:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: demo-app
  labels:
    app: demo
spec:
  replicas: 2
  selector:
    matchLabels:
      app.kubernetes.io/name: demo-app
      app.kubernetes.io/instance: instance
  template:
    metadata:
      labels:
        app.kubernetes.io/name: demo-app
        app.kubernetes.io/instance: instance
    spec:
      containers:
      - name: event-proxy
        image: arcusazure/arcus-event-grid-proxy
        env:
        - name: ARCUS_EVENTGRID_TOPICENDPOINT
          value: https://arcus.io
        - name: ARCUS_EVENTGRID_AUTHKEY
          valueFrom:
            secretKeyRef:
             name: secrets-order-consumer
             key: servicebus-connectionstring
        - name: KUBERNETES_NODE_NAME
          valueFrom:
            fieldRef:
             fieldPath: spec.nodeName
        - name: KUBERNETES_POD_NAME
          valueFrom:
            fieldRef:
             fieldPath: metadata.name
        - name: KUBERNETES_NAMESPACE
          valueFrom:
            fieldRef:
             fieldPath: metadata.namespace
```

### Custom Serilog property names

The Kubernetes enricher allows you to specify the names of the log properties that will be added to the log event during enrichment.
By default the node name is set to `NodeName`, the pod name to `PodName`, and the namespace to `Namespace`.

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithKubernetesInfo(
        nodeNamePropertyName: "MyNodeName",
        podNamePropertyName: "MyPodName",
        namespacePropertyName: "MyNamespace")
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {MyNodeName: demo-cluster, MyPodName: demo-app, MyNamespace: demo}
```

## Version Enricher

The `Arcus.Observability.Telemetry.Serilog.Enrichers` library provides a [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment) 
that adds (by default) the current runtime assembly version of the product to the log event as a log property with the name `version`.

**Example**
Name: `version`
Value: `1.0.0-preview`

**Usage**

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithVersion()
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {version: 1.0.0-preview}
```

### Custom application version

The version enricher allows you to specify an `IAppVersion` instance that retrieves your custom application version, which will be used during enrichement.
By default this is set to the version of the current executing assembly.

**Assembly version as application version**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register the `AssemblyAppVersion` instance to retrieve the application version from the assembly where the passed-along `Startup` type is located.
    services.AddAssemblyAppVersion<Startup>();
}
```

**User-provided version**

```csharp
using Arcus.Observability.Telemetry.Core;
using Serilog;

IAppVersion appVersion = new MyCustomAppVersion("v0.1.0");
ILogger logger = new LoggerConfiguration()
    .Enrich.WithVersion(appVersion)
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {version: v0.1.0}
```

Or alternatively, you can choose to register the application version so you can use it in your application as well.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Startup
{
    public void ConfigureServivces(IServiceCollection services)
    {
        // Register the `MyApplicationVersion` instance to the registered services (using empty constructor).
        services.AddAppVersion<MyApplicationVersion>();

        // Register the `MyApplicationVersion` instance using the service provider.
        services.AddAppVersion(serviceProvider => 
        {
            var logger = serviceProvider.GetRequiredService<ILogger<MyApplicationVersion>>();
            return new MyApplicationVersion(logger);
        });
    }
}
```

Once the application version is registered, you can pass along the `IServiceProvider` instead to the Serilog configuration.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Serilog;

IServiceProvider serviceProvider = ...
ILogger logger = new LoggerConfiguration()
    .Enrich.WithVersion(serviceProvider)
    .CreateLogger();
```

### Custom Serilog property names

The version enricher allows you to specify the name of the property that will be added to the log event during enrichement.
By default this is set to `version`.

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithVersion(propertyName: "MyVersion")
    .CreateLogger();

logger.Information("Some event");
// Output: Some event {MyVersion: 1.0.0-preview}
```

[&larr; back](/)
