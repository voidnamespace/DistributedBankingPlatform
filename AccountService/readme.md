# AccountService

## Overview

AccountService is part of a distributed, event-driven banking platform. It manages account data, exposes an ASP.NET Core Web API for account operations, persists state in PostgreSQL through EF Core, and exchanges integration events through RabbitMQ topic exchanges.

The service follows Clean Architecture and is split into four projects:

- `AccountService.Domain`
- `AccountService.Application`
- `AccountService.Infrastructure`
- `AccountService.API`

Core stack:

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Serilog
- Docker Compose

## Responsibilities

The current codebase is responsible for:

- Creating accounts
- Activating and deactivating accounts
- Deleting accounts
- Tracking account balances
- Querying accounts by id, account number, current user, or as an admin list
- Processing integration events from other services
- Publishing account-related integration events

## Architecture

### Domain

Contains the core business model:

- `Account` aggregate
- Value objects such as `AccountNumberVO` and `MoneyVO`
- Domain events for account lifecycle and balance changes
- Domain rules such as active-state checks, balance checks, and currency validation

### Application

Contains use cases and orchestration:

- Commands and queries handled through MediatR
- Validation pipeline with FluentValidation
- Domain event handlers that write integration events to the outbox
- Integration event handlers that react to messages received from RabbitMQ

### Infrastructure

Contains technical integrations:

- EF Core `AccountDbContext`
- PostgreSQL persistence and migrations
- Repository and unit of work implementations
- RabbitMQ consumers and publisher
- Inbox and outbox persistence/processors

### API

Contains the HTTP surface and host setup:

- ASP.NET Core controllers
- JWT authentication and authorization
- Swagger/OpenAPI
- Health checks
- IP rate limiting
- Serilog configuration

## Messaging Topology

RabbitMQ uses topic exchanges.

### Exchanges and queues configured in `appsettings.json`

- Consumes from `auth.events` using queue `account.auth.events`
- Consumes from `transaction.events` using queue `account.transaction.events`
- Publishes to `account.events`

### Routing keys currently bound by the consumers

From `auth.events`:

- `user.*`

This includes the events you called out:

- `user.created`
- `user.deleted`

The current event type map also includes:

- `user.activated`
- `user.deactivated`

From `transaction.events`:

- `transfer.*`
- `withdrawal.*`
- `deposit.*`

### Account events currently published by the service

The outbox publisher can emit:

- `account.created`
- `account.activated`
- `account.deactivated`
- `account.deleted`
- `account.balance`

The events you explicitly listed are implemented:

- `account.created`
- `account.deleted`

## Database

The service uses PostgreSQL with EF Core and applies migrations on startup.

### Main tables

- `Accounts`
- `InboxMessages`
- `OutboxMessages`

### Account persistence details

The `Accounts` table stores:

- Account id
- User id
- Unique account number
- Balance amount
- Balance currency
- `IsActive`
- `CreatedAt`
- `UpdatedAt`
- `RowVersion` for optimistic concurrency

### Messaging persistence

- `InboxMessages` stores consumed integration events before they are processed
- `OutboxMessages` stores integration events before they are published to RabbitMQ

## Background Processors

The service registers the following hosted services:

- `AuthEventsConsumer`
- `TransactionEventsConsumer`
- `InboxProcessor`
- `OutboxProcessor`

What they do:

- Consumers receive RabbitMQ messages and store them in the inbox
- `InboxProcessor` reads unprocessed inbox rows, resolves the integration event type, and dispatches it through MediatR
- Domain event handlers enqueue outgoing integration events into the outbox as part of the same persistence flow
- `OutboxProcessor` polls unprocessed outbox rows and publishes them to RabbitMQ

## Reliability Guarantees

The current implementation includes these reliability mechanisms:

- Durable RabbitMQ topic exchanges and durable queues
- Persistent published RabbitMQ messages
- Inbox deduplication by `MessageId`
- Inbox persistence before event handling
- Outbox persistence before broker publishing
- Retry-friendly inbox and outbox processors that keep failed messages with attempt count and error details
- Optimistic concurrency on accounts through `RowVersion`

In practice this means:

- Incoming messages are acknowledged only after they are saved to the inbox
- Outgoing integration events are first stored in the database, then published asynchronously by the outbox processor
- Failed inbox/outbox processing attempts are not lost; they remain in the database for later retries

## Configuration

Main configuration lives in `src/AccountService.API/appsettings.json`.

### Important settings

- `ConnectionStrings:AccountDb`
- `RabbitMq:Host`
- `RabbitMq:Port`
- `RabbitMq:Username`
- `RabbitMq:Password`
- `AuthEventsConsumer:Exchange`
- `AuthEventsConsumer:Queue`
- `TransactionEventsConsumer:Exchange`
- `TransactionEventsConsumer:Queue`
- `AccountEventsPublisher:Exchange`
- `Jwt:SecretKey`
- `Jwt:Issuer`
- `Jwt:Audience`
- `IpRateLimiting`

### Default local values

- PostgreSQL: `localhost:5433`
- RabbitMQ AMQP: `localhost:5672`
- RabbitMQ management UI: `http://localhost:15672`
- API through Docker Compose: `http://localhost:8081`

## How to run locally

### Option 1: Run with Docker Compose

1. From the repository root, start the stack:

```powershell
docker compose -f docker-compose.local.yml up --build
```

2. Open the API at:

- `http://localhost:8081`

3. Open RabbitMQ management UI at:

- `http://localhost:15672`

Default credentials:

- Username: `guest`
- Password: `guest`

### Option 2: Run the API locally against local dependencies

1. Start PostgreSQL and RabbitMQ locally so they match the values in `appsettings.json`.
2. From the repository root, run the API project:

```powershell
dotnet run --project src/AccountService.API
```

### Startup behavior

On startup, the API:

- Configures Serilog logging to console and rolling log files under `logs/`
- Applies pending EF Core migrations
- Starts HTTP endpoints
- Starts RabbitMQ consumers and inbox/outbox processors

### Useful endpoints

- Swagger UI in development: `/`
- Health: `/health`
- Readiness: `/health/ready`
- Liveness: `/health/live`
