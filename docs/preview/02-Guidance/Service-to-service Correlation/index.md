---
title: "Service-to-service Correlation?"
layout: default
---

# Service-to-service correlation
The concept of service-to-service correlation is big and complex and spans multiple Arcus libraries. This user-guide will walk through all the available Arcus features that work together to facilitate service-to-service correlation in your application.

## What is service-to-service correlation?
Service-to-service correlation is a way to describe a relationship between different components where each 'component' represents a separated application. This could be, for example, an API call that results in an Azure Service Bus message that will be picked up by a message pump, or Azure Service Bus Azure Function that calls an additional API after receiving a message from a queue.

Anywhere where one application calls another, that's where service-to-service correlation comes in. This way of correlation makes sure that the relationship of a single (business) transaction doesn't stop at the borders of one application or component, but continues in the other application. The end result is a clear overview of all the components involved.

![Arcus service-to-service application map](/media/service-to-service-correlation-application-map.png)

When using Arcus for service-to-service correlation, you'll see the this relationship in Application Insights in the [end-to-end transaction view](https://docs.microsoft.com/en-us/azure/azure-monitor/app/transaction-diagnostics):

![Arcus service-to-service correlation relationship](/media/service-to-service-correlation-relationship.png)

## Why should I use service-to-service correlation?
Adding service-to-service correlation to your application adds many advantages:
* ✔ Quicker spot performance bottlenecks and failures
* ✔ Clear overview of used (Azure) resources (cost effective)
* ✔ Effective architectural decision-making on application flow

## What Arcus components support service-to-service correlation?
Currently, we support service-to-service correlation with two types of components:
* [Arcus Web API](https://webapi.arcus-azure.net/features/correlation)
* [Arcus Azure Service Bus message pump/router](https://messaging.arcus-azure.net/)
* And [other built-in and custom dependencies](https://observability.arcus-azure.net/Features/writing-different-telemetry-types)

We're working on adding Azure EventHubs message pump to the mix.

> ⚠ Service-to-service correlation is only built-in available for Azure Functions that run in-process. Out-of-process / isolated Azure Functions doesn't have (yet) the necessary built-in Arcus functionality to facilitate service-to-service correlation.

> ⚠ Service-to-service correlation is currently only supported for projects that uses Application Insights as their Serilog logging system. If you want to use Arcus' service-to-service functionality outside Application Insights, [please let us know](https://github.com/arcus-azure/arcus.observability/issues/new/choose).

## How do I add service-to-service correlation to my application components?
Service-to-service correlation should be added on both the sending and receiving components of your project so the internal Arcus functionality can link the request/response correctly in Application Insights. The following user-guides will go over the sending and receiving side of Web API's and Azure Service Bus resources:

* [Use service-to-service correlation to Web API solutions](use-service-to-service-correlation-in-web-api.md)