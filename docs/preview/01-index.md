---
title: "Arcus Observability"
layout: default
slug: /
sidebar_label: Welcome
---

# Introduction

Arcus Observability allows you to work easily with Azure Application Insights telemetry. Instead of managing logging, correlation, tracking, Arcus Observability allows you to work with the common `ILogger` infrastructure and still be able to write multi-dimensional telemetry data. The library supports multiple telemetry types like tracking dependencies, requests, events, metrics, while also be able to filter with Serilog filters and enrich with custom correlation.

![Logger Arcus - Application Insights](/img/logger-arcus-appinsights.png)

# Guidance

- [Using Arcus & Serilog in ASP.NET Core](./03-Guidance/use-with-dotnet-and-aspnetcore.md)
- [Using Arcus & Serilog in Azure Functions](./03-Guidance/use-with-dotnet-and-functions.md)

# Installation

The Arcus.Observability.Correlation package can be installed via NuGet:

```shell
PM > Install-Package Arcus.Observability.Serilog.Sinks.ApplicationInsights
```

For more granular packages we recommend reading [the documentation](./02-Features/sinks/azure-application-insights.md).

# License
This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Codit is the original author of this web application.

*[Full license here](https://github.com/arcus-azure/arcus.observability/blob/master/LICENSE)*
