---
title: "Arcus Observability"
layout: default
slug: /
sidebar_label: Welcome
---

[![NuGet Badge](https://buildstats.info/nuget/Arcus.Observability.Correlation?packageVersion=2.2.0)](https://www.nuget.org/packages/Arcus.Observability.Correlation/2.2.0)

# Installation

The Arcus.Observability.Correlation package can be installed via NuGet:

```shell
PM > Install-Package Arcus.Observability.Correlation --Version 2.2.0
```

For more granular packages we recommend reading the documentation.

# Features

- [Making telemetry more powerful](/features/making-telemetry-more-powerful) by making it simple to provide contextual information
- [Writing different telemetry types](/features/writing-different-telemetry-types) - Go beyond logs with our `ILogger` extensions for Dependencies, Events, Requests & Metrics.
- [Correlation](/features/correlation) - A common set of correlation levels.
- Telemetry
    - [Enrichment](/features/telemetry-enrichment) - A set of enrichers to improve telemetry information.
    - [Filters](/features/telemetry-filter) - A set of filters to control telemetry flow with.
- Sinks
    - [Azure Application Insights](/features/sinks/azure-application-insights) - Flow Traces, Dependencies, Events, Requests & Metrics information to Azure Application Insights

# Guidance

- [Using Arcus & Serilog in .NET Core and/or Azure Functions](/guidance/use-with-dotnet-and-functions)

# License
This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Codit is the original author of this web application.

*[Full license here](https://github.com/arcus-azure/arcus.observability/blob/master/LICENSE)*
