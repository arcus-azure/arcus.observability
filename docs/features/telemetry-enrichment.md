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