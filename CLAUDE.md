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
dotnet run --project .\app\src\ChatApp.Api\ChatApp.Api.csproj

# Start only the database (required before running locally)
docker-compose up -d --build chatapp-db

# EF migrations
dotnet ef migrations add <MigrationName> --project .\app\src\ChatApp.Infrastructure\ChatApp.Infrastructure.csproj --startup-project .\app\src\ChatApp.Api\
dotnet ef database update --project .\app\src\ChatApp.Infrastructure\ --startup-project .\app\src\ChatApp.Api\
```

## Architecture

Clean Architecture with four layers. Dependencies flow inward: **API → Application → Domain** (Infrastructure implements Application interfaces).

- **Domain** (`ChatApp.Domain`): Entities, value objects, repository interfaces. No framework dependencies. Entities use private setters and static factory methods (`ChatRoom.Create()`, `ChatMessage.Create()`).
- **Application** (`ChatApp.Application`): CQRS use cases via MediatR, organized under `UseCases/{Feature}/{UseCase}/`. Defines abstractions (`IUserRepository`, `IChatHub`, etc.) that Infrastructure implements.
- **Infrastructure** (`ChatApp.Infrastructure`): EF Core + PostgreSQL, SignalR hub, JWT auth, AWS S3 file storage, rate limiting. Registered in `DependencyInjection.cs`.
- **API** (`ChatApp.Api`): Controllers dispatch commands/queries via `ISender`. Middlewares: exception handling, IP logging, request context logging (correlation ID).

## Key Patterns

**Result pattern** — all use case handlers return `Result` or `Result<T>`. Always check `result.IsFailure` before accessing `result.Value`. Domain-level errors are typed `Error` records with a `Code` and `Name`.

**CQRS via MediatR** — commands implement `ICommand<TResponse>`, queries implement `IQuery<TResponse>`. Handlers implement `ICommandHandler<,>` / `IQueryHandler<,>`. The `LoggingBehavior` pipeline behavior runs on every request.

**IUserContext** — injects the current authenticated user's `UserId` (from JWT claims) into handlers. Handlers call `_userContext.UserId` rather than reading claims directly.

**IUnitOfWork** — `await _unitOfWork.Commit(cancellationToken)` must be called at the end of every write handler to persist changes.

**Domain entities** — use private constructors; instantiate via static `Create()` factory methods. Mutations return `Result` when they can fail (e.g., `ChatMessage.Edit()`).

## Real-Time

SignalR hub at `/chatHub`. The `IChatHub` interface (in Application) is implemented by `SignalRChatRoomNotifier` (Infrastructure), which is what command handlers call. `ChatHub` (the actual SignalR hub class) handles client connections and group management.

## Testing

Unit tests only (`ChatApp.UnitTests`). Stack: **xUnit + NSubstitute + FluentAssertions**. Test names are written in Portuguese. Tests mock all dependencies via `NSubstitute.Substitute.For<T>()` and instantiate the handler directly — no DI container in tests.

## Configuration

`appsettings.json` requires:
- `ConnectionStrings:Database` — PostgreSQL connection string
- `JwtSettings:SecretKey` — symmetric key for JWT signing
- `AwsSettings:S3` — `BucketName`, `Region`, and optionally `AccessKey`/`SecretKey`

CORS allows origins: `localhost:3000`, `localhost:5173`, `localhost:4200`.

Migrations run automatically in Development via `app.ApplyMigrations()` in `Program.cs`.
