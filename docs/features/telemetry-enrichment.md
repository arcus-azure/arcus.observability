---
title: "Home"
layout: default
---

# Telemetry Enrichment

## Version Enricher

The `Arcus.Observability.Telemetry.Serilog` library provides a [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment) 
that adds the current runtime assembly version of the product to the log event as a log property with the name `version`.

**Example**
Name: `version`
Value: `1.0.0-preview`

**Usage**

```csharp
ILogger logger = new LoggerConfiguration()
    .Enrich.With<VersionEnricher>()
    .CreateLogger();

logger.Information("This event will be enriched with the runtime assembly product version");
```

## Kubernetes Enricher

The `Arcus.Observability.Telemetry.Serilog` library provides a [Kubernetes](https://kubernetes.io/) [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment) 
that adds several machine information from the environment (variables).

**Example**
| Environment Variable   | Log Property |
| ---------------------- | ------------ |
| `KUBERNETES_NODE_NAME` | NodeName     |
| `KUBERNETES_POD_NAME`  | PodName      |
| `KUBERNETES_NAMESPACE` | Namespace    |

**Usage**

```csharp
ILogger logger = new LoggerConfiguration()
    .Enrich.With<KubernetesEnricher>()
    .CreateLogger();

logger.Information("This event will be enriched with the Kubernetes environment information");
```

## Application Enricher

The `Arcus.Observability.Telemetry.Serilog` library provides a [Serilog enricher](https://github.com/serilog/serilog/wiki/Enrichment)
that adds the application's component name to the log event as a log property with the name `ComponentName`.

**Example**
Name: `ComponentName`
Value: `My application component`

**Usage**

```csharp
ILogger logger = new LoggerConfiguration()
    .Enrich.With(new ApplicationEnricher("My application component"))
    .CreateLogger();

logger.Information("This event will be enriched with the application component's name");
```