---
title: "Home"
layout: default
permalink: /
redirect_from:
 - /index.html
---

[![NuGet Badge](https://buildstats.info/nuget/Arcus.Observability.Correlation?packageVersion=0.1.1)](https://www.nuget.org/packages/Arcus.Observability.Correlation/0.1.1)

# Installation

The Arcus.Observability.Correlation can be installed via NuGet:

```shell
PM > Install-Package Arcus.Observability.Correlation -Version 0.1.0
```

For more granular packages we recommend reading the documentation.

# Features

- [Writing different telemetry types](/features/writing-different-telemetry-types) - Go beyond logs with our `ILogger` extensions for Dependencies, Events, Requests & Metrics.
- [Correlation](/features/correlation) - A common set of correlation levels.
- Telemetry
    - [Enrichment](/features/telemetry-enrichment) - A set of enrichers to improve telemetry information.
    - [Filters](/features/telemetry-filter) - A set of filters to control telemetry flow with.
- Sinks
    - [Azure Application Insights](/features/sinks/azure-application-insights) - Flow Traces, Dependencies, Events, Requests & Metrics information to Azure Application Insights

# License
This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Codit is the original author of this web application.

*[Full license here](https://github.com/arcus-azure/arcus.observability/blob/master/LICENSE)*