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
| Environment Variable   | Log Property                                 | Value                |
| ---------------------- | -------------------------------------------- | -------------------- |
| `KUBERNETES_NODE_NAME` | (default: same name as environment variable) | `spec.nodeName`      |
| `KUBERNETES_POD_NAME`  | (default: same name as environment variable) | `metadata.name`      |
| `KUBERNETES_NAMESPACE` | (default: same name as environment variable) | `metadata.namespace` |

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

The name of each log property can be configured too:

```csharp
ILogger logger = new LoggerConfiguration()
    .Enrich.With(new KubernetesEnricher(
        new Dictionary<string, string>
        {
            ["My_KUBERNETES_NODE_NAME"] = "MyKubernetesNodeName",
            ["MY_KUBERNETES_POD_NAME"] = "MyKubernetesPodName",
            ["MY_KUBERNETES_NAMESPACE"] = "MyKubernetesNamespace"
        }))
    .CreateLogger();

logger.Information("This event will be enriched with the Kubernetes environment information");
```