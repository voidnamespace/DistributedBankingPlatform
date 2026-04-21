# TransactionService

## Overview

`TransactionService` is part of a distributed, event-driven banking platform. It exposes an ASP.NET Core Web API for creating and tracking transactions, persists transaction data in PostgreSQL via Entity Framework Core, and communicates with other services through RabbitMQ topic exchanges.

The service is built on .NET 9 and follows Clean Architecture with separate `Domain`, `Application`, `Infrastructure`, and `API` layers.

## Responsibilities

This service is responsible for:

- Processing money transfers between accounts
- Processing deposits
- Processing withdrawals
- Persisting transaction history
- Publishing transaction-related integration events
- Processing integration events received from other services and updating transaction state

At the moment, the HTTP API exposes these endpoints:

- `POST /api/transaction/transfer`
- `POST /api/transaction/withdrawal`
- `POST /api/transaction/deposit`
- `GET /api/transaction/{transactionId}`

`transfer` and `withdrawal` endpoints require JWT authentication. `deposit` and status lookup are currently exposed without `[Authorize]`.

## Architecture

The service uses Clean Architecture with four layers:

- `Domain`
  Contains core transaction entities, value objects, enums, domain exceptions, and domain events.
- `Application`
  Contains commands, queries, validators, integration events, domain event handlers, integration event handlers, and application interfaces.
- `Infrastructure`
  Contains EF Core persistence, repositories, unit of work, RabbitMQ publishing/consuming, inbox/outbox persistence, and hosted background processors.
- `API`
  Contains controllers, middleware, authentication setup, Swagger configuration, and application startup.

Transaction creation follows this flow:

1. The API receives a request and dispatches a MediatR command.
2. The application layer creates a `Transaction` aggregate in `Processing` state.
3. Domain events raised by the aggregate are handled inside the unit of work.
4. Corresponding integration events are written to the outbox.
5. `OutboxProcessor` publishes pending outbox messages to RabbitMQ.
6. Incoming account-related events are stored in the inbox and processed asynchronously to update transaction status.

## Messaging Topology

RabbitMQ topic exchanges are used for service-to-service communication.

### Published exchange

`TransactionService` publishes to:

- `transaction.events`

Currently implemented outgoing routing keys are:

- `transfer.created`
- `withdrawal.created`
- `deposit.created`

These events are first stored in the outbox and then published by the background outbox processor.

### Consumed exchange

`TransactionService` consumes from:

- `account.events`

The service declares and consumes the queue:

- `transaction.account.events`

The queue is bound to these routing key patterns:

- `transfer.*`
- `withdrawal.*`
- `deposit.*`

Incoming messages are first stored in the inbox, then processed by the inbox processor through MediatR notification handlers.

### Incoming event handling

Based on the handlers currently present in the codebase, the service reacts to:

- `transfer.success`
- `transfer.failed`
- `withdrawal.success`
- `withdrawal.failed`
- `deposit.success`

These events are used to mark transactions as completed or failed in the local database.

## Database

The service uses PostgreSQL with Entity Framework Core.

The `TransactionDbContext` manages these tables:

- `Transactions`
- `InboxMessages`
- `OutboxMessages`

The database stores:

- Transaction records and their current status
- Incoming integration messages tracked by the inbox pattern
- Outgoing integration messages tracked by the outbox pattern

EF Core migrations are applied automatically on startup in the API layer.

## Background Processors

Three hosted background services are registered:

- `AccountEventsConsumer`
  Connects to RabbitMQ, consumes messages from `account.events`, and persists them into the inbox.
- `InboxProcessor`
  Polls unprocessed inbox records, deserializes them, and dispatches them to application integration event handlers.
- `OutboxProcessor`
  Polls unprocessed outbox records and publishes them to `transaction.events`.

`OutboxProcessor` polls every second. `InboxProcessor` polls every five seconds.

## Reliability Guarantees

The service implements both the Outbox and Inbox patterns.

- Outbox pattern:
  Outgoing integration events are written to `OutboxMessages` during the same application workflow as transaction state changes, then published asynchronously by `OutboxProcessor`.
- Inbox pattern:
  Incoming RabbitMQ messages are stored in `InboxMessages` before business handling. Duplicate message IDs are skipped by `InboxWriter`.
- Durable messaging:
  Exchanges, queues, and published messages are configured as durable/persistent.
- Retry behavior:
  When inbox or outbox processing fails, the message remains unprocessed, the attempt count is incremented, and the error is stored for a later retry cycle.
- Startup migrations:
  The service applies EF Core migrations automatically when the API starts.

This gives the service at-least-once style processing for both inbound and outbound messaging, with persistence-backed retry loops.

## Configuration

Main configuration lives in [appsettings.json](src/TransactionService.API/appsettings.json).

Important settings include:

- `ConnectionStrings:TransactionDb`
- `RabbitMq:Host`
- `RabbitMq:Port`
- `RabbitMq:Username`
- `RabbitMq:Password`
- `AccountEventsConsumer:Exchange`
- `AccountEventsConsumer:Queue`
- `TransactionEventsPublisher:Exchange`
- `Jwt:SecretKey`
- `Jwt:Issuer`
- `Jwt:Audience`

Logging is configured with Serilog and writes to:

- Console
- `logs/log-.txt`

Swagger is enabled in the Development environment.

## How To Run Locally

### Prerequisites

- .NET 9 SDK
- Docker and Docker Compose

### 1. Start infrastructure

Run:

```powershell
docker compose -f Docker-compose.local.yml up -d
```

This starts:

- PostgreSQL on host port `5432`
- RabbitMQ on host port `5672`
- RabbitMQ Management UI on `http://localhost:15672`

### 2. Verify database connection settings

The default connection string in [appsettings.json](src/TransactionService.API/appsettings.json) uses PostgreSQL port `5434`, while [Docker-compose.local.yml](Docker-compose.local.yml) exposes PostgreSQL on `5432`.

Before starting the API, make sure those values match. For example, update the connection string to use:

```text
Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres
```

### 3. Run the API

From the repository root, run:

```powershell
dotnet run --project src/TransactionService.API
```

In Development, Swagger UI is available at the application root. The launch settings define:

- `http://localhost:5176`
- `https://localhost:7090`

### 4. Migrations

No separate migration step is required for local startup. The service calls `Database.Migrate()` automatically during application startup.
