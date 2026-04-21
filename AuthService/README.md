# AuthService

AuthService is the identity and authentication microservice of a distributed, event-driven banking platform. It is responsible for user account lifecycle operations, JWT issuance, and publishing user-related integration events for downstream services.

The service is built as a production-style backend component using Clean Architecture, reliable messaging patterns, and containerized infrastructure for local development.

## Overview

AuthService provides the authentication boundary of the platform. It manages user registration and account state changes, issues JWT access tokens, persists operational data in PostgreSQL, and emits integration events through RabbitMQ so other microservices can react to user lifecycle changes asynchronously.

Core technology stack:

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- RabbitMQ with topic exchanges
- Redis
- Serilog
- MediatR
- Outbox pattern
- Docker Compose

## Responsibilities

AuthService is responsible for:

- User registration
- User activation
- User deactivation
- JWT token issuing
- Publishing user lifecycle integration events

Integration events published by this service:

- `user.created`
- `user.activated`
- `user.deactivated`
- `user.deleted`

## Architecture

The service follows Clean Architecture and is organized into four layers:

### Domain

Contains the core business model and domain rules:

- Entities
- Value objects
- Domain events
- Domain-specific enums and invariants

### Application

Contains use case orchestration and application logic:

- Commands and queries
- MediatR handlers
- Validation pipeline behaviors
- Integration event definitions
- Abstractions for infrastructure concerns

### Infrastructure

Contains technical implementations and external integrations:

- Entity Framework Core persistence
- PostgreSQL access
- RabbitMQ publishing
- Redis integration
- JWT generation and validation
- Outbox processing
- Logging and infrastructure wiring

### API

Contains the HTTP entry point of the service:

- Controllers
- Request/response contracts
- Validation integration
- Middleware
- Dependency registration and startup configuration

## Messaging Topology

AuthService publishes integration events to the RabbitMQ topic exchange:

- Exchange: `auth.events`

Routing keys used by the service:

- `user.created`
- `user.activated`
- `user.deactivated`
- `user.deleted`

This contract allows other services in the platform to subscribe only to the user lifecycle events they need.

Example topology:

```text
AuthService
  -> RabbitMQ topic exchange: auth.events
       -> user.created
       -> user.activated
       -> user.deactivated
       -> user.deleted
```

## Database

AuthService stores operational data in PostgreSQL and uses Entity Framework Core as the ORM.

Key database characteristics:

- PostgreSQL is the primary relational store
- EF Core migrations are applied automatically on startup
- Persistence is handled in the Infrastructure layer
- The database is part of the local Docker Compose environment

## Background Processors

AuthService uses background processing to support reliable asynchronous workflows:

- `OutboxProcessor` reads pending integration events from the outbox store and publishes them to RabbitMQ

These processors allow the service to decouple request handling from asynchronous message delivery and consumption concerns.

## Reliability Guarantees

Reliable messaging is a core part of the service design.

### Outbox Pattern

Outgoing integration events are first stored in the same transactional boundary as the business state change. A background processor then publishes them to RabbitMQ.

This provides:

- Reduced risk of losing integration events after a successful database transaction
- Controlled retry behavior for message publishing
- Safer eventual consistency across microservices

## JWT

AuthService issues JWT access tokens for authenticated users.

Token characteristics:

- Includes `user id`
- Includes `role`
- Lifetime: `1 hour`

JWT configuration includes:

- Signing secret
- Issuer
- Audience

## Logging

The service uses Serilog for structured application logging.

Logging is configured for:

- Console output
- Rolling file logs

This makes local development and operational troubleshooting easier while keeping logging concerns centralized.

## Configuration

Configuration is provided through ASP.NET Core configuration sources such as `appsettings.json` and environment variables.

Important configuration areas:

- PostgreSQL connection string
- RabbitMQ connection settings
- Redis connection settings
- JWT settings
- Exchange name for integration event publishing

## Environment Variables

The following environment variables are relevant for local execution and containerized startup:

```env
ASPNETCORE_ENVIRONMENT=Development

ConnectionStrings__AuthDb=Host=postgres;Port=5432;Database=authdb;Username=postgres;Password=postgres
Redis=redis:6379

RabbitMq__Host=rabbitmq
RabbitMq__Port=5672
RabbitMq__Username=guest
RabbitMq__Password=guest
RabbitMq__ChannelPoolMaxSize=16

AuthEventsPublisher__Exchange=auth.events

Jwt__SecretKey=YourStrongSecretKeyHere
Jwt__Issuer=AuthService
Jwt__Audience=AuthServiceClient
```

Default local infrastructure services:

- PostgreSQL
- RabbitMQ
- Redis

## How To Run Locally

### 1. Start infrastructure

Run Docker Compose to start PostgreSQL, RabbitMQ, and Redis:

```powershell
docker compose -f docker-compose.local.yml up --build
```

### 2. Run the API

If you want to run the API directly from the host instead of inside the container:

```powershell
dotnet run --project .\src\AuthService.API\AuthService.API.csproj
```

### 3. Access the service

By default, the local containerized API is exposed on:

- `http://localhost:8080`

In development mode, Swagger UI is enabled by the API project and can be used for manual endpoint testing.

## Summary

AuthService is a focused authentication microservice designed for a distributed banking platform. It combines Clean Architecture, PostgreSQL persistence, JWT-based authentication, RabbitMQ integration, and Outbox reliability patterns to support production-style backend workflows in an event-driven microservices environment.
