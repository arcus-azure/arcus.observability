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
that adds the application's component name to the log event as a log property with the name `ComponentName`.

**Example**
Name: `ComponentName`
Value: `My application component`

**Usage**

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithComponentName("My application component")
    .CreateLogger();

logger.Information("This event will be enriched with the application component's name");
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
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithCorrelationInfo()
    .CreateLogger();

logger.Information("This event will be enriched with the correlation information");
```

Or alternatively, with a custom `ICorrelationInfoAccessor`:

```csharp
using Arcus.Observability.Correlation;
using Serilog;

ICorrelationInfoAccessor myCustomAccessor = ...

ILogger logger = new LoggerConfiguration()
    .Enrich.WithCorrelationInfo(myCustomAccessor)
    .CreateLogger();

logger.Information("This event will be enriched with the correlation information");
// Output: This event will be enriched with the correlation information {OperationId: 52EE2C00-53EE-476E-9DAB-C1234EB4AD0B, TransactionId: 0477E377-414D-47CD-8756-BCBE3DBE3ACB}
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

logger.Information("This event will be enriched with the Kubernetes environment information");
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

## Version Enricher

The `Arcus.Observability.Telemetry.Serilog.Enrichers` library provides a [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment) 
that adds the current runtime assembly version of the product to the log event as a log property with the name `version`.

**Example**
Name: `version`
Value: `1.0.0-preview`

**Usage**

```csharp
using Serilog;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithVersion()
    .CreateLogger();

logger.Information("This event will be enriched with the runtime assembly product version");
```

[&larr; back](/)
