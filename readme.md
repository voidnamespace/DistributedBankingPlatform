\# Distributed Banking Platform (Work in Progress)



This repository contains a distributed event-driven backend platform built with multiple .NET microservices.



The project is currently under active development and is not yet assembled into a single fully orchestrated runtime environment.



Each service is implemented and documented independently.



Please read the README inside each service directory for implementation details.



\---



\## Current State



At the moment:



\* services are implemented as separate microservices

\* RabbitMQ topic exchanges are used for service-to-service communication

\* Inbox / Outbox reliability patterns are implemented where required

\* AuditLogService captures the full integration event stream

\* FeeService is still under development

\* shared infrastructure runs through docker-compose

\* full unified system orchestration is still being finalized



Because of this, services are intended to be started individually during development.



\---



\## Infrastructure Setup



Start shared infrastructure:



```

docker compose up -d

```



This starts:



\* RabbitMQ (AMQP + management UI)

\* Redis

\* PostgreSQL instances (per service)

\* MongoDB (AuditLogService storage)

\* Prometheus (metrics groundwork)



Infrastructure containers are shared across services.



\---



\## Running Services Locally



Run services individually from their API projects.



Example:



```

dotnet run --project src/AuthService.API

```



Repeat for other services as needed:



\* AuthService

\* AccountService

\* TransactionService

\* AuditLogService

\* FeeService (partial implementation)



Each service README explains its responsibilities and messaging topology.



\---



\## Recommended Exploration Flow



Suggested order for reviewing the system:



1\. AuthService

2\. AccountService

3\. TransactionService

4\. AuditLogService

5\. FeeService



Each service README documents:



\* architecture layers

\* integration events

\* persistence strategy

\* background processors

\* reliability patterns



\---



\## Important Note



This repository represents an evolving architecture-focused backend system rather than a finished production-ready platform.



Service boundaries, orchestration flow, and observability integration are still being extended incrementally.



The intended way to understand the project is to review services individually and follow the integration events between them.



