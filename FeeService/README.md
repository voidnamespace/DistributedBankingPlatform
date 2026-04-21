# FeeService

`FeeService` is a .NET 9 service in a distributed, event-driven banking platform. Based on the current code, its implemented role is to keep fee-related user state in sync with user lifecycle events coming from the auth domain.

This README is based only on the code in this repository. If something is not described here as implemented, it is because it is not currently present in code.

## Current status

### Implemented

- ASP.NET Core host with dependency injection and a `/health` endpoint.
- PostgreSQL persistence through Entity Framework Core.
- Automatic EF Core migration execution on service startup.
- RabbitMQ consumption through MassTransit.
- A database-backed inbox pattern for reliable asynchronous processing.
- Handling of these auth/user integration events:
  - `user.created`
  - `user.activated`
  - `user.deactivated`
  - `user.deleted`
- Creation and deletion of `UserMaintenanceFeeState` records based on those events.
- Dead-letter storage for inbox messages that fail processing repeatedly.

### Planned or not yet completed

- Actual fee charging workflow for maintenance fees.
- Any scheduler or worker that charges users when `NextChargeAt` is reached.
- Transfer-threshold fee tracking. There is only a placeholder `UserTransferThresholdFeeState` class, and it is not wired into persistence or processing.
- Outgoing event publishing from this service.
- Business HTTP API endpoints beyond the health check.
- Any authentication/authorization behavior beyond the middleware being registered.

## Service purpose

Today, the service acts primarily as a fee-state synchronizer for user lifecycle events.

When auth-related events arrive, the service stores them in an inbox table and processes them asynchronously. Processing currently keeps a `UserMaintenanceFeeState` row per active user:

- `user.created`: creates a maintenance fee state if one does not already exist.
- `user.activated`: creates a maintenance fee state if one does not already exist.
- `user.deactivated`: removes the maintenance fee state if it exists.
- `user.deleted`: removes the maintenance fee state if it exists.

The domain entity also contains `ChargedAt`, `NextChargeAt`, and `MarkCharged(...)`, which suggests the intended next step is recurring maintenance fee charging, but that behavior is not implemented yet.

## Architecture layers

The repository follows a layered structure under `src/`:

### `FeeService.API`

- Application entry point.
- Registers infrastructure services, controllers, and health checks.
- Exposes `/health`.
- Does not currently contain any controllers or business endpoints.

### `FeeService.Application`

- Defines application-facing contracts:
  - `IInboxWriter`
  - `IInboxDispatcher`
  - `IInboxMessageHandler<T>`
- Defines integration event contracts consumed by the service.
- Does not currently contain use-case services or command/query handlers.

### `FeeService.Domain`

- Contains domain entities and domain-event base abstractions.
- Implemented entity:
  - `UserMaintenanceFeeState`
- Placeholder only:
  - `UserTransferThresholdFeeState`

### `FeeService.Infrastructure`

- Entity Framework Core database integration.
- RabbitMQ/MassTransit wiring.
- Inbox persistence and dispatching.
- Background services:
  - database migrator
  - inbox processor
- Concrete inbox handlers for user lifecycle events.

## Messaging topology

The current messaging topology is hardcoded in infrastructure:

- Broker: RabbitMQ
- Transport library: MassTransit
- Receive endpoint / queue: `fee.auth.events`
- Bound exchange: `auth.events`
- Exchange type: `topic`
- Routing key binding: `user.*`

### Inbound message flow

1. MassTransit receives one of the supported user integration events from RabbitMQ.
2. The consumer serializes the message payload and writes it to `InboxMessages`.
3. `InboxProcessor` polls the inbox table every 5 seconds.
4. Messages are selected in batches of up to 20 using `FOR UPDATE SKIP LOCKED`.
5. `InboxDispatcher` resolves the message type and invokes the matching inbox handler.
6. The handler updates fee-state data in PostgreSQL.
7. On repeated failure, the message is copied to `DeadLetterInboxMessages` after 5 attempts.

### Supported inbound event types

- `UserCreatedIntegrationEvent`
- `UserActivatedIntegrationEvent`
- `UserDeactivatedIntegrationEvent`
- `UserDeletedIntegrationEvent`

### Not implemented

- No event publishing/outbox flow exists in the current code.
- No additional exchanges, queues, or consumers are implemented beyond the auth event subscription above.

## Database usage

Database provider: PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`

The service uses a single EF Core `FeeDbContext`.

### Tables currently represented in code and migrations

- `UserMaintenanceFeeStates`
  - Primary key: `UserId`
  - Columns:
    - `ChargedAt`
    - `NextChargeAt`
    - `CreatedAt`
- `InboxMessages`
  - Stores received but not-yet-processed integration messages.
  - Tracks message type, payload, processing status, attempt count, timestamps, and last error.
- `DeadLetterInboxMessages`
  - Stores inbox messages that exceeded the retry limit.

### Migration behavior

- Migrations are applied automatically on service startup by `FeeDbContextMigrator`.
- The migrator retries database startup failures up to 10 times with a 3-second delay.
- A design-time `FeeDbContextFactory` is included for EF Core tooling.

### Not implemented

- No persistence mapping exists for `UserTransferThresholdFeeState`.
- No database tables for executed fee charges, invoices, balances, or audit history exist in this repository.

## Background processors

### `FeeDbContextMigrator`

- Runs on startup.
- Applies EF Core migrations automatically.
- Retries transient startup failures.

### `InboxProcessor`

- Runs continuously as a hosted background service.
- Poll interval: 5 seconds.
- Batch size: 20 messages.
- Max attempts before dead-lettering: 5.
- Uses a database transaction and `SKIP LOCKED` selection to safely process pending messages.

### Not implemented

- No recurring billing scheduler.
- No worker that scans `NextChargeAt` and applies charges.
- No reconciliation or replay processor beyond the inbox retry/dead-letter mechanism.

## Configuration

Current configuration is read from `src/FeeService.API/appsettings.json` and environment-specific overrides.

### Connection strings

- `ConnectionStrings:FeeDb`
  - Default: `Host=localhost;Port=5435;Database=feedb;Username=postgres;Password=postgres`

### RabbitMQ

- `RabbitMq:Host`
- `RabbitMq:Port`
- `RabbitMq:Username`
- `RabbitMq:Password`

These settings are actively used by the MassTransit/RabbitMQ configuration.

### Other settings present in config

- `AuthEventsConsumer:Exchange`
- `AuthEventsConsumer:Queue`

These settings exist in `appsettings.json`, but the current code does not read them. The queue name, exchange name, and routing key binding are currently hardcoded in infrastructure.

### Development launch profile

`launchSettings.json` sets:

- `ASPNETCORE_ENVIRONMENT=Development`
- `MT_LICENSE=Community`
- HTTP URL: `http://localhost:5023`
- HTTPS URL: `https://localhost:7015`

## HTTP surface

Currently implemented:

- `GET /health`

Not currently implemented:

- No REST API controllers
- No fee management endpoints
- No admin endpoints

## How to run locally

### Prerequisites

- .NET SDK 9
- PostgreSQL
- RabbitMQ

### Start PostgreSQL

The repository includes a local Docker Compose file for PostgreSQL only:

```bash
docker compose -f docker-compose.local.yml up -d
```

This starts PostgreSQL on `localhost:5435` with:

- database: `feedb`
- username: `postgres`
- password: `postgres`

### Start RabbitMQ

RabbitMQ is required by the service, but it is not provisioned in `docker-compose.local.yml`.

The default expected connection settings are:

- host: `localhost`
- port: `5672`
- username: `guest`
- password: `guest`

You need to start RabbitMQ separately before running the service.

### Run the service

From the repository root:

```bash
dotnet run --project src/FeeService.API/FeeService.API.csproj
```

On startup, the service will:

1. connect to PostgreSQL
2. apply EF Core migrations automatically
3. connect to RabbitMQ
4. start the inbox processor
5. expose the health endpoint

### Verify it is running

Check:

- `http://localhost:5023/health`

## Summary of implemented scope

The current implementation is an infrastructure-heavy foundation for fee processing, not a complete fee engine yet.

Implemented today:

- consume auth user lifecycle events
- persist them to an inbox
- process them reliably in the background
- maintain `UserMaintenanceFeeState` records for active users

Not completed yet:

- charging fees
- scheduling future charges
- transfer-threshold fee logic
- business APIs
- outbound messaging from FeeService
