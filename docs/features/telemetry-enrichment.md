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

The `Arcus.Observability.Telemetry.Serilog` library provides a [Kubernetes](https://kubernetes.io/) enricher that adds several machine information from the environment (variables).

**Example**
| Environment Variable   | Log Property | Value                |
| ---------------------- | ------------ | -------------------- |
| `KUBERNETES_NODE_NAME` | NodeName     | `spec.nodeName`      |
| `KUBERNETES_POD_NAME`  | PodName      | `metadata.name`      |
| `KUBERNETES_NAMESPACE` | Namespace    | `metadata.namespace` |

**Usage**

```csharp
ILogger logger = new LoggerConfiguration()
    .Enrich.With<KubernetesEnricher>()
    .CreateLogger();

logger.Information("This event will be enriched with the Kubernetes environment information");
```

The name of each environment variable can be configured:

```csharp
ILogger logger = new LoggerConfiguration()
    .Enrich.With(new KubernetesEnricher(
        "My_KUBERNETES_NODE_NAME",
        "MY_KUBERNETES_POD_NAME",
        "MY_KUBERNETES_NAMESPACE"))
    .CreateLogger();

logger.Information("This event will be enriched with the Kubernetes environment information");
```