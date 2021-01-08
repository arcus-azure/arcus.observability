---
title: "Home"
layout: default
redirect_from:
 - /index.html
---

[![NuGet Badge](https://buildstats.info/nuget/Arcus.Observability.Correlation?includePreReleases=true)](https://www.nuget.org/packages/Arcus.Observability.Correlation/)

# Installation

The Arcus.Observability.Correlation can be installed via NuGet:

```shell
PM > Install-Package Arcus.Observability.Correlation
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

- [Using Arcus & Serilog in .NET Core and/or Azure Functions](/guidance/use-with-dotnet-and-functions.md)

# License
This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Codit is the original author of this web application.

*[Full license here](https://github.com/arcus-azure/arcus.observability/blob/master/LICENSE)*

# Older Versions

- [v1.0](v1.0)
- [v0.4](v0.4)
- [v0.3](v0.3)
- [v0.2.0](v0.2.0)
- [v0.1.1](v0.1.1)
- [v0.1.0](v0.1.0)
