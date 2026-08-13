# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```sh
# Restore, build, test
dotnet restore
dotnet build
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~ConversationTest"

# Run the API (Swagger at http://localhost:5000/swagger)
dotnet run --project .\app\Api\ChatApp.Api\ChatApp.Api.csproj

# Start only the database (required before running locally)
docker-compose up -d --build chat-db

# EF migrations — one DbContext per module, each in its own schema.
# The design-time factories read DATABASE_CONNECTION_STRING, so no startup project is needed.
dotnet ef migrations add <Name> --project .\app\Modules\Chat\src\Chat.Infrastructure\
dotnet ef migrations add <Name> --project .\app\Modules\Identity\src\Identity.Infrastructure\
dotnet ef database update --project .\app\Modules\Chat\src\Chat.Infrastructure\
dotnet ef database update --project .\app\Modules\Identity\src\Identity.Infrastructure\
```

Do **not** pass `--no-build` when regenerating migrations after deleting the `Migrations/` folder — EF would read the stale snapshot from the previously compiled assembly and scaffold a diff instead of a clean initial migration.

## Architecture

**Modular Monolith** with Clean Architecture per module and **Minimal APIs** (no Controllers). The solution file `ChatApp.slnx` is at the repo root; code lives under `app/`:

- `app/Api/ChatApp.Api` — single host that only **composes** modules. No business logic; owns cross-cutting middleware (Serilog, CORS, versioning, Swagger, **rate limiter**, `ApplyMigrations` for both DbContexts). `AddIdentityModule` must be registered **before** `AddChatModule` — Identity owns the authentication scheme.
- `app/Modules/{Chat,Identity,Notification}` — each module has its own `src/` (four layers: `Domain`, `Application`, `Infrastructure`, `Presentation`) and `tests/`. **Chat** and **Identity** are implemented; Notification is still a scaffold.
- `app/Shared/SharedKernel` — `Result`, `Error`, `Entity`, `AggregateRoot`, `IDomainEvent`.
- `app/Shared/BuildingBlocks` — CQRS messaging (`ICommand`/`IQuery`/handlers), pipeline behaviors, `IDateTimeProvider`, `IUserContext`. No ASP.NET dependency.
- `app/Shared/BuildingBlocks.Api` — `ApiResults` (Result → HTTP) and `ValidationFilter<T>`, shared by every module's Presentation layer.

### Module boundary

**Identity owns users and authentication** (`identity` schema, `IdentityDbContext`): `User`, register/login/search, JWT issuing and validation, `IUserContext`.

**Chat never joins `identity.Users`** — it stores only `UserId` and resolves names/avatars through `Identity.Contracts`:

- `IUserDirectory.GetByIds(ids)` — batch hydration, used by the conversation list. Always batch; never call it per row.
- `IUserDirectory.Exists(userId)` — validating a DM target or new group members.
- `IUserPresenceSink.TouchLastSeen(userId, at)` — the hub reports disconnects; Identity persists "last seen".

Each module has its **own** unit-of-work type (`IUnitOfWork` for Chat, `IIdentityUnitOfWork` for Identity). They are deliberately distinct types: a single shared interface would make the two DI registrations collide.

Chat module layers (dependencies flow inward: **Presentation/API → Application → Domain**; Infrastructure implements Application interfaces):

- **Domain** (`Chat.Domain`): `Conversations/` (`Conversation`, `Participant`) and `Messages/` (`Message`, `MessageContent`, `ContentType`). Private constructors + static factories; every mutation that can fail returns `Result`.
- **Application** (`Chat.Application`): CQRS use cases under `UseCases/{Feature}/{UseCase}/`, plus abstractions (`IConversationDao`, `IChatNotifier`, `IPresenceTracker`, `IFileStorageService`).
- **Infrastructure** (`Chat.Infrastructure`): EF Core + PostgreSQL (`chat` schema), Dapper read models, SignalR, AWS S3.
- **Presentation** (`Chat.Presentation`): Minimal API endpoints dispatching via `ISender`; `ChatModule` exposes `AddChatModule`/`MapChatEndpoints` (which also maps the SignalR hub).

## Conversation model

`Conversation` has a `Type` — `Direct` (1x1) or `Group` — and the type governs every invariant:

- **Direct**: exactly two participants, immutable membership, no name and no admins. `DirectKey` is a deterministic ordered pair (`BuildDirectKey(a, b)`) with a **filtered unique index**, so opening a DM twice returns the same conversation. Attempts to add/remove/rename fail with `Conversation.DirectIsImmutable`.
- **Group**: name required, one `Owner`, `Admin`s, members join/leave. The owner cannot leave without `TransferOwnership` first.

`Participant` carries the **read/delivery cursors** (`LastReadMessageSentAt`, `LastDeliveredMessageSentAt`) — these hold the *`SentAt` of the last read message*, not the ack instant. That is what makes read receipts work without a message × user table:

- unread count = messages with `SentAt > LastReadMessageSentAt`
- ✓ sent · ✓✓ delivered (all others' delivery cursor ≥ `SentAt`) · ✓✓ blue (all others' read cursor ≥ `SentAt`)

Cursors are **monotonic** — an out-of-order ack from another device can never move them backwards.

Messages are **soft-deleted** (`DeletedAt`); a hard delete would put holes between the read cursor and the unread count. Media messages persist `StorageKey` (the S3 key) separately from the presigned URL — deletion needs the key, not the URL.

## Key Patterns

**Result pattern** — all use case handlers return `Result` or `Result<T>`. Always check `result.IsFailure` before accessing `result.Value`. Domain-level errors are typed `Error` records with a `Code` and `Name`.

**CQRS via MediatR** — commands implement `ICommand<TResponse>`, queries implement `IQuery<TResponse>`. Handlers implement `ICommandHandler<,>` / `IQueryHandler<,>`. The `LoggingBehavior` pipeline behavior runs on every request.

**IUserContext** — injects the current authenticated user's `UserId` (from JWT claims) into handlers. Handlers call `_userContext.UserId` rather than reading claims directly.

**IUnitOfWork** — `await _unitOfWork.Commit(cancellationToken)` must be called at the end of every write handler to persist changes.

**Domain entities** — use private constructors; instantiate via static `Create()` factory methods. Mutations return `Result` when they can fail (e.g., `Message.Edit()`). Permission checks (admin, owner, membership) live **in the aggregate**, not in handlers — handlers load, delegate, persist.

**Returning success** — where the method returns `Result<T>`, prefer the implicit conversion (`return message;`) over `Result.Success(message)`, matching the failure side (`return MessageErrors.NotFound;`). This does **not** work when `T` is an interface (e.g. `IReadOnlyList<T>`): C# forbids user-defined conversions *from* an interface type, so those still need `Result.Success(...)`.

**EF + domain-generated ids** — aggregates assign their own `Guid` in the constructor, so every key mapping must declare `.ValueGeneratedNever()`. Without it EF assumes the database generates the key, and attaching a *new* child (e.g. a `Participant`) to an *already-tracked* aggregate makes EF read "key is set ⇒ existing row" and emit an `UPDATE` that affects 0 rows. For the same reason, repository `Update()` methods only call `DbSet.Update` when the entity is `Detached` — on a tracked graph it would force everything to `Modified`.

**Read models** — list endpoints go through Dapper DAOs (`IConversationDao`, `IMessageDao`), not EF. Keyset pagination is always `ORDER BY "SentAt" DESC, "Id" DESC` (the `Id` is the tiebreaker) and `take` is clamped in the handler.

## Real-Time

SignalR hub at `/chatHub`, typed as `Hub<IChatClient>`. Three rules:

1. **Writes never go through the hub.** The hub only exposes `Typing` and connection lifecycle. Sending a message is HTTP → handler → database → notifier.
2. **Notifications are raised from domain-event handlers** (`Chat.Application/UseCases/RealTime/`), not scattered across command handlers. Recipients are re-read from the conversation at send time, never carried inside the event.
3. **Delivery is user-addressed** (`Clients.Users(...)`), not group-based. `NameClaimType` is `ClaimTypes.NameIdentifier`, so SignalR's default `IUserIdProvider` resolves the user id — no group bookkeeping, survives reconnects, reaches every device.

`IChatNotifier` (Application) is the only contract; `SignalRChatNotifier` implements it and `IChatClient` defines the canonical client event names.

JWT for WebSockets requires the `OnMessageReceived` handler in `Identity.Infrastructure/DependencyInjection.cs` that reads `access_token` from the query string — the browser cannot set an `Authorization` header on the WebSocket handshake.

Presence lives in `InMemoryPresenceTracker` and is **process-local**: correct for a single instance only. Scaling horizontally requires a Redis backplane (`AddStackExchangeRedis`) plus a distributed presence store.

## Testing

Each module keeps its own tests under `app/Modules/<Module>/tests/` — `Chat.UnitTests`, `Chat.IntegrationTests` (boots the host via `WebApplicationFactory` against a Testcontainers Postgres, migrating **both** DbContexts) and `Identity.UnitTests`. The repo-root `tests/` folder is reserved for cross-cutting **architecture** and **end-to-end** tests.

Integration tests need Docker running. `IntegrationTestBase.CreateUserAsync()` returns a `TestUser` with an authenticated `HttpClient` and the real `UserId`.

`RealTime/RealTimeTests.cs` drives a real `HubConnection` against the `TestServer`. Two details make it work: `SkipNegotiation` + WebSockets transport, and a `WebSocketFactory` that appends `access_token` to the query string itself. The .NET client would normally send the token as an `Authorization` header, which `TestServer.CreateWebSocketClient()` silently drops — and a header would bypass the very code path (`OnMessageReceived`) that browsers depend on.

Stack: **xUnit + NSubstitute + FluentAssertions**. Test names are written in Portuguese. Unit tests mock all dependencies via `NSubstitute.Substitute.For<T>()` and instantiate the handler directly — no DI container.

## Configuration

`appsettings.json` requires:
- `ConnectionStrings:Database` — PostgreSQL connection string
- `JwtSettings:SecretKey` — symmetric key for JWT signing
- `AwsSettings:S3` — `BucketName`, `Region`, and optionally `AccessKey`/`SecretKey`

CORS allows origins: `localhost:3000`, `localhost:5173`, `localhost:4200`.

Migrations run automatically in Development via `app.ApplyMigrations()` in `Program.cs` (Identity first, then Chat). The rate limiter is registered by the host and disabled in Development.

## API surface (v1)

```
POST   /api/v1/auth/register · /api/v1/auth/login
GET    /api/v1/users?search= · /api/v1/users/me

POST   /api/v1/conversations/direct           { targetUserId }   ← idempotent
POST   /api/v1/conversations/group            { name, memberIds[] }
GET    /api/v1/conversations?before=&take=    ← the home screen
GET    /api/v1/conversations/{id}
PATCH  /api/v1/conversations/{id}             { name }
POST   /api/v1/conversations/{id}/participants
DELETE /api/v1/conversations/{id}/participants/me | /{userId}
POST   /api/v1/conversations/{id}/participants/{userId}/promote | /demote
POST   /api/v1/conversations/{id}/read        { lastMessageId }
GET    /api/v1/conversations/{id}/messages?before=&take=
POST   /api/v1/conversations/{id}/messages
PUT    /api/v1/messages/{id}  ·  DELETE /api/v1/messages/{id}  ·  POST /api/v1/messages/upload
```

Media flow: `POST /messages/upload` returns `{ url, storageKey, fileName, sizeBytes }`; the client echoes those back in `POST /conversations/{id}/messages`.
