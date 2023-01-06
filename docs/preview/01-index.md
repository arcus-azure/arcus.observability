---
title: "Arcus Observability"
layout: default
slug: /
sidebar_label: Welcome
---

# Introduction

Arcus Observability allows you to work easily with Azure Application Insights telemetry by making use of the common `ILogger` infrastructure to track dependencies, log custom metrics and log multi-dimensional telemetry data. The library supports multiple telemetry types like tracking dependencies, requests, events, metrics, while also be able to filter with Serilog filters and enrich with custom correlation.

![Logger Arcus - Application Insights](/img/logger-arcus-appinsights.png)

# Guidance

- [Using Arcus & Serilog in ASP.NET Core](./02-Guidance/use-with-dotnet-and-aspnetcore.md)
- [Using Arcus & Serilog in Azure Functions](./02-Guidance/use-with-dotnet-and-functions.md)

# Installation

The Arcus.Observability.Correlation package can be installed via NuGet:

```shell
PM > Install-Package Arcus.Observability.Serilog.Sinks.ApplicationInsights
```

For more granular packages we recommend reading [the documentation](./03-Features/sinks/azure-application-insights.md).

# License
This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Codit is the original author of this web application.

*[Full license here](https://github.com/arcus-azure/arcus.observability/blob/master/LICENSE)*
