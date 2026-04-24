# Rwd.WF API

Workflow and lookup management API built with Clean Architecture + CQRS on .NET 9.

## Tech Stack

- ASP.NET Core Web API
- MediatR (CQRS)
- FluentValidation
- Entity Framework Core + PostgreSQL
- Redis (caching)
- RabbitMQ + MassTransit
- Swagger / OpenAPI

## Solution Structure

```text
src/
  Rwd.WF.API/            # Presentation layer (controllers, auth, middleware)
  Rwd.WF.Application/    # Use-cases, commands/queries, DTOs, behaviors
  Rwd.WF.Domain/         # Domain entities, events, repository contracts
  Rwd.WF.Infrastructure/ # EF Core, repositories, unit of work, seeding, integrations
```

## Architecture Notes

- **Clean Architecture**: dependencies point inward (`API -> Application -> Domain`, `Infrastructure -> Application/Domain`).
- **CQRS**: write/read flows are handled with MediatR commands and queries.
- **Read/Write persistence split**:
  - `WorkflowWriteDbContext` for writes.
  - `WorkflowReadDbContext` for reads with `NoTracking`.
  - Shared model configuration in `WorkflowDbContext`.
- **Unit of Work**: `IUnitOfWork` implemented by `UnitOfWork` and uses the write context.

## Prerequisites

- .NET SDK 9.0+
- Docker Desktop (for local infra via compose)
- Optional: PostgreSQL, Redis, RabbitMQ, Seq if running without Docker

## Configuration

Main settings file: `src/Rwd.WF.API/appsettings.json`

Important keys:

- `ConnectionStrings:Postgres`
- `ConnectionStrings:Redis`
- `RabbitMQ:Host`, `RabbitMQ:VHost`, `RabbitMQ:Username`, `RabbitMQ:Password`
- `IAM:Authority`, `IAM:Audience`
- `Seq:Url`

## Run with Docker Compose

From solution root:

```bash
docker compose up --build
```

This starts:

- PostgreSQL (`5432`)
- Redis (`6379`)
- RabbitMQ (`5672`, management `15672`)
- API (`7100 -> 8080`)

## Run Locally (without Docker)

1. Make sure dependencies are reachable (Postgres/Redis/RabbitMQ/Seq).
2. Update `src/Rwd.WF.API/appsettings.Development.json` or environment variables.
3. Run:

```bash
dotnet restore Rwd.WF.sln
dotnet build Rwd.WF.sln
dotnet run --project src/Rwd.WF.API/Rwd.WF.API.csproj
```

Swagger UI will be available at:

- `https://localhost:<port>/`
- `http://localhost:<port>/`

(`RoutePrefix` is empty in `Program.cs`, so Swagger opens at root in Development/Staging.)

## Database Migration and Seeding

On application startup, infrastructure seeding is executed:

- `app.Services.SeedInfrastructureAsync()`
- Applies EF migrations.
- Seeds default lookup data (idempotent):
  - Categories: `WORKFLOW_STATUS`, `PRIORITY`
  - Items:
    - `WORKFLOW_STATUS`: `DRAFT`, `PUBLISHED`, `ARCHIVED`
    - `PRIORITY`: `LOW`, `MEDIUM`, `HIGH`

Seeder location:

- `src/Rwd.WF.Infrastructure/Persistence/Seeding/WorkflowDbSeeder.cs`
- `src/Rwd.WF.Infrastructure/Persistence/Seeding/LookupDefaultsSeeder.cs`

## API Endpoints (v1)

Base path includes API version in URL: `api/v{version}`

Lookup categories:

- `GET    /api/v1/lookup-categories`
- `GET    /api/v1/lookup-categories/{id}`
- `POST   /api/v1/lookup-categories`
- `PUT    /api/v1/lookup-categories/{id}`
- `DELETE /api/v1/lookup-categories/{id}`

Lookup items:

- `GET    /api/v1/lookup-items?categoryId={guid?}`
- `GET    /api/v1/lookup-items/{id}`
- `POST   /api/v1/lookup-items`
- `PUT    /api/v1/lookup-items/{id}`
- `DELETE /api/v1/lookup-items/{id}`

## Authorization

JWT Bearer authentication is enabled.

Lookup policies/permissions:

- `lookup:read`
- `lookup:create`
- `lookup:update`
- `lookup:delete`

Configured in `src/Rwd.WF.API/Authorization`.

## Logging and Observability

- Structured logs with Serilog.
- Console sink + Seq sink.
- Global exception handling via `GlobalExceptionMiddleware`.

## Build

```bash
dotnet build Rwd.WF.sln
```

