# AuditLogService

## Overview

`AuditLogService` is part of a distributed event-driven banking platform. Its current implementation consumes integration events from RabbitMQ and stores the raw event payloads for audit and traceability purposes.

The solution is organized using Clean Architecture-style projects:

- `AuditLogService.Domain`
- `AuditLogService.Application`
- `AuditLogService.Infrastructure`
- `AuditLogService.API`

At the moment, most of the implemented behavior lives in the `Infrastructure` and `API` projects. The `Domain` and `Application` projects are present but still minimal.

## Responsibilities

The service currently:

- Connects to RabbitMQ and listens for events from multiple topic exchanges
- Stores each received event as an audit record
- Preserves the event payload and routing key for traceability
- Runs the event consumer as a background hosted service

## Architecture

The repository is split into four layers:

- `Domain`: placeholder project for core domain concepts
- `Application`: placeholder project for application use cases
- `Infrastructure`: messaging and persistence implementation
- `API`: application entry point and dependency wiring

Current flow:

1. The API project starts the host.
2. Infrastructure registers the RabbitMQ consumer and MongoDB persistence services.
3. A background service connects to RabbitMQ and binds a durable queue to the configured exchanges.
4. When a message is received, the service stores it in the database and acknowledges the message only after persistence succeeds.

## Messaging Topology

The service is configured to consume from these topic exchanges:

- `auth.events`
- `account.events`
- `transaction.events`

It declares and consumes a single durable queue:

- `auditlog.events`

Current binding behavior:

- The queue is bound to each exchange with routing key `#`

That means the service currently receives all routing keys published to those exchanges, including event families such as:

- `user.*`
- `account.*`
- `transfer.*`
- `withdrawal.*`
- `deposit.*`

## Database

The current implementation uses MongoDB, not PostgreSQL.

Stored events are written to the configured MongoDB collection with the following fields:

- `Id`
- `Type` - populated from the RabbitMQ routing key
- `Payload` - raw message body as UTF-8 text
- `ReceivedAt` - UTC timestamp when the message was received

Configured defaults from `appsettings.json`:

- Connection string: `mongodb://localhost:27017`
- Database: `auditlogdb`
- Collection: `events`

There is no EF Core setup or automatic migration logic in the current codebase.

## Background Processors

The service contains one background processor:

- `AuditLogEventsConsumer`

Its responsibilities are:

- Retry RabbitMQ connection startup until the broker becomes available
- Declare the configured exchanges and queue
- Consume messages with manual acknowledgements
- Persist each event to MongoDB
- Acknowledge successful messages
- Requeue failed messages with `BasicNack(..., requeue: true)`

## Reliability Guarantees

The current implementation provides these guarantees:

- Durable exchanges and queue are declared on startup
- Messages are consumed with manual acknowledgements
- A message is acknowledged only after it is successfully stored
- Failed processing results in the message being requeued
- Consumer prefetch is set to `1`, so messages are processed one at a time per consumer

Important limitations of the current implementation:

- There is no implemented Inbox persistence model with deduplication/idempotency metadata
- Because failed messages are requeued, duplicate processing is possible
- There is no dead-letter handling in this repository

## Configuration

Configuration is currently read from `src/AuditLogService.API/appsettings.json`.

Available sections:

- `RabbitMQ`
  - `Host`
  - `Port` is present in configuration but is not currently used by the connection factory
  - `Username`
  - `Password`
- `AuthEvents`
  - `Exchange`
- `AccountEvents`
  - `Exchange`
- `TransactionEvents`
  - `Exchange`
- `AuditLogEvents`
  - `Queue`
- `Mongo`
  - `ConnectionString`
  - `Database`
  - `Collection`

## How To Run Locally

### Prerequisites

- .NET 9 SDK
- RabbitMQ running locally on `localhost:5672`
- MongoDB running locally on `localhost:27017`

### Run the service

From the repository root:

```powershell
dotnet restore
dotnet run --project .\src\AuditLogService.API\AuditLogService.API.csproj
```

### Local behavior

On startup, the service will:

- Build the ASP.NET Core host
- Register infrastructure services
- Start the background RabbitMQ consumer
- Declare the exchanges and queue if they do not already exist
- Begin storing incoming messages in MongoDB

## Notes On Current State

This README reflects the code that is currently implemented in this repository. The following items mentioned in the target platform description are not present in the current codebase:

- PostgreSQL
- Entity Framework Core
- Automatic EF Core migrations
- Serilog
- Docker Compose files
- HTTP API endpoints
- A fully implemented Inbox pattern beyond storing incoming messages before acknowledgement
