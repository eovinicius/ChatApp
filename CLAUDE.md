# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```sh
# Restore, build, test
dotnet restore
dotnet build
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~CreateRoomTests"

# Run the API (Swagger at http://localhost:5110/swagger/index.html)
dotnet run --project .\app\Api\ChatApp.Api\ChatApp.Api.csproj

# Start only the database (required before running locally)
docker-compose up -d --build chat-db

# EF migrations
dotnet ef migrations add <MigrationName> --project .\app\Modules\Chat\src\Chat.Infrastructure\Chat.Infrastructure.csproj --startup-project .\app\Api\ChatApp.Api\
dotnet ef database update --project .\app\Modules\Chat\src\Chat.Infrastructure\ --startup-project .\app\Api\ChatApp.Api\
```

## Architecture

**Modular Monolith** with Clean Architecture per module and **Minimal APIs** (no Controllers). The solution file `ChatApp.slnx` is at the repo root; code lives under `app/`:

- `app/Api/ChatApp.Api` — single host that only **composes** modules (`AddChatModule` + `MapChatEndpoints`, plus Identity/Notification stubs). No business logic; owns cross-cutting middleware (Serilog, CORS, versioning, Swagger, rate limiter, auth, `ApplyMigrations`).
- `app/Modules/{Chat,Identity,Notification}` — each module has its own `src/` (four layers: `Domain`, `Application`, `Infrastructure`, `Presentation`) and `tests/`. Only **Chat** is implemented; Identity and Notification are structured scaffolds (compile + compose, no logic yet).
- `app/Shared/SharedKernel` — `Result`, `Error`, `Entity`, `AggregateRoot`, `IDomainEvent` (shared across modules).
- `app/Shared/BuildingBlocks` — CQRS messaging (`ICommand`/`IQuery`/handlers), pipeline behaviors, `IDateTimeProvider`.

Chat module layers (dependencies flow inward: **Presentation/API → Application → Domain**; Infrastructure implements Application interfaces):

- **Domain** (`Chat.Domain`): Entities, value objects, repository interfaces. Entities use private setters and static factory methods (`ChatRoom.Create()`, `ChatMessage.Create()`).
- **Application** (`Chat.Application`): CQRS use cases via MediatR, organized under `UseCases/{Feature}/{UseCase}/`. Defines abstractions (`IUserRepository`, `IChatHub`, etc.) that Infrastructure implements.
- **Infrastructure** (`Chat.Infrastructure`): EF Core + PostgreSQL, SignalR hub, JWT auth, AWS S3 file storage, rate limiting. Registered in `DependencyInjection.cs`.
- **Presentation** (`Chat.Presentation`): Minimal API endpoint classes (`Endpoints/`, `Requests/`) that dispatch commands/queries via `ISender`; `ChatModule` exposes `AddChatModule`/`MapChatEndpoints` (which also maps the SignalR hub).

## Key Patterns

**Result pattern** — all use case handlers return `Result` or `Result<T>`. Always check `result.IsFailure` before accessing `result.Value`. Domain-level errors are typed `Error` records with a `Code` and `Name`.

**CQRS via MediatR** — commands implement `ICommand<TResponse>`, queries implement `IQuery<TResponse>`. Handlers implement `ICommandHandler<,>` / `IQueryHandler<,>`. The `LoggingBehavior` pipeline behavior runs on every request.

**IUserContext** — injects the current authenticated user's `UserId` (from JWT claims) into handlers. Handlers call `_userContext.UserId` rather than reading claims directly.

**IUnitOfWork** — `await _unitOfWork.Commit(cancellationToken)` must be called at the end of every write handler to persist changes.

**Domain entities** — use private constructors; instantiate via static `Create()` factory methods. Mutations return `Result` when they can fail (e.g., `ChatMessage.Edit()`).

## Real-Time

SignalR hub at `/chatHub`. The `IChatHub` interface (in Application) is implemented by `SignalRChatRoomNotifier` (Infrastructure), which is what command handlers call. `ChatHub` (the actual SignalR hub class) handles client connections and group management.

## Testing

Each module keeps its own tests under `app/Modules/<Module>/tests/` — for **Chat**: `Chat.UnitTests` (unit) and `Chat.IntegrationTests` (integration, boots the host via `WebApplicationFactory` against a Testcontainers Postgres). The repo-root `tests/` folder is reserved for cross-cutting **architecture** and **end-to-end** tests.

Stack: **xUnit + NSubstitute + FluentAssertions**. Test names are written in Portuguese. Unit tests mock all dependencies via `NSubstitute.Substitute.For<T>()` and instantiate the handler directly — no DI container.

## Configuration

`appsettings.json` requires:
- `ConnectionStrings:Database` — PostgreSQL connection string
- `JwtSettings:SecretKey` — symmetric key for JWT signing
- `AwsSettings:S3` — `BucketName`, `Region`, and optionally `AccessKey`/`SecretKey`

CORS allows origins: `localhost:3000`, `localhost:5173`, `localhost:4200`.

Migrations run automatically in Development via `app.ApplyMigrations()` in `Program.cs`.
