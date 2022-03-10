---
title: "Arcus Observability"
layout: default
slug: /
sidebar_label: Welcome
---

[![NuGet Badge](https://buildstats.info/nuget/Arcus.Observability.Correlation?packageVersion=0.2.0)](https://www.nuget.org/packages/Arcus.Observability.Correlation/0.2.0)

# Installation

The Arcus.Observability.Correlation package can be installed via NuGet:

```shell
PM > Install-Package Arcus.Observability.Correlation -Version 0.2.0
```

For more granular packages we recommend reading the documentation.

# Features

- [Making telemetry more powerful](./02-Features/making-telemetry-more-powerful.md) by making it simple to provide contextual information
- [Writing different telemetry types](./02-Features/writing-different-telemetry-types.md) - Go beyond logs with our `ILogger` extensions for Dependencies, Events, Requests & Metrics.
- [Correlation](./02-Features/correlation.md) - A common set of correlation levels.
- Telemetry
    - [Enrichment](./02-Features/telemetry-enrichment.md) - A set of enrichers to improve telemetry information.
    - [Filters](./02-Features/telemetry-filter.md) - A set of filters to control telemetry flow with.
- Sinks
    - [Azure Application Insights](./02-Features/sinks/azure-application-insights.md) - Flow Traces, Dependencies, Events, Requests & Metrics information to Azure Application Insights

# License
This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Codit is the original author of this web application.

*[Full license here](https://github.com/arcus-azure/arcus.observability/blob/master/LICENSE)*
