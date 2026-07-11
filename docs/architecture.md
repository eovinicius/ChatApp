# Arquitetura

## Visão Geral

O sistema é um **Monólito Modular**: `ChatApp.Api` apenas compõe módulos independentes (`Chat`, `Identity`, `Notification`). Cada módulo segue **Clean Architecture** em quatro camadas (`Domain`, `Application`, `Infrastructure`, `Presentation`), expondo endpoints via **Minimal APIs**. Primitivas compartilhadas ficam em `Shared/SharedKernel` e `Shared/BuildingBlocks`. O diagrama abaixo detalha o módulo **Chat**:

```mermaid
graph TD
    subgraph Externas
        Host["ChatApp.Api\ncomposição dos módulos"]
        Pres["Chat.Presentation\nMinimal API Endpoints · Hub"]
        Infra["Chat.Infrastructure\nEF Core · SignalR · JWT · S3"]
    end
    subgraph Internas
        App["Chat.Application\nUse Cases · CQRS · Abstrações"]
        Domain["Chat.Domain\nEntidades · Value Objects · Erros"]
    end

    Host --> Pres
    Pres -->|"ISender (MediatR)"| App
    Infra -->|"implementa interfaces"| App
    App --> Domain
    Infra --> Domain
```

## Camadas

| Camada | Projeto | Responsabilidade |
|--------|---------|-----------------|
| **Domain** | `Chat.Domain` | Entidades, value objects, interfaces de repositórios. Sem dependências de framework. |
| **Application** | `Chat.Application` | Use cases via CQRS (MediatR). Define abstrações (`IUserRepository`, `IChatHub`, etc.) que Infrastructure implementa. |
| **Infrastructure** | `Chat.Infrastructure` | EF Core + PostgreSQL, SignalR, JWT, AWS S3. Implementações concretas das abstrações de Application. |
| **Presentation** | `Chat.Presentation` | Endpoints Minimal API (`Endpoints/`, `Requests/`). Despacha comandos/queries via `ISender`; expõe `AddChatModule`/`MapChatEndpoints`. |
| **Host** | `ChatApp.Api` | Composição dos módulos + middlewares/configuração transversal (Serilog, CORS, versionamento, Swagger, rate limiter, auth, migrations). |

## Fluxo de uma Request HTTP

```mermaid
sequenceDiagram
    participant C as Controller
    participant M as MediatR
    participant B as Behaviors
    participant H as Handler
    participant R as Repository
    participant DB as PostgreSQL

    C->>M: sender.Send(command)
    M->>B: LoggingBehavior
    B->>H: Handle(command, ct)
    H->>R: repository.GetByIdAsync(id)
    R->>DB: SELECT ...
    DB-->>R: entity
    R-->>H: entity
    H->>R: repository.AddAsync(newEntity)
    H->>M: unitOfWork.Commit(ct)
    M-->>C: Result<T>
```

## Padrões Fundamentais

### Result Pattern

Todos os handlers retornam `Result` ou `Result<T>`. Erros de negócio nunca lançam exceções — são representados como `Error` records com `Code` e `Name`.

```csharp
// No handler
var room = await _roomRepository.GetByIdAsync(command.RoomId, ct);
if (room is null)
    return Result.Failure<Guid>(ChatRoomErrors.NotFound);

// No controller
var result = await _sender.Send(command);
if (result.IsFailure)
    return BadRequest(result.Error);
return Ok(result.Value);
```

### CQRS via MediatR

Commands alteram estado; queries apenas lêem. Cada use case tem seu próprio handler em `UseCases/{Feature}/{UseCase}/`.

```
UseCases/
├── Rooms/
│   ├── CreateRoom/
│   │   ├── CreateRoomCommand.cs         ← ICommand<Guid>
│   │   └── CreateRoomCommandHandler.cs  ← ICommandHandler<CreateRoomCommand, Guid>
│   └── JoinRoom/
│       ├── JoinRoomCommand.cs
│       └── JoinRoomCommandHandler.cs
└── Messages/
    └── GetMessagesByRoom/
        ├── GetMessagesByRoomQuery.cs         ← IQuery<IReadOnlyList<...>>
        └── GetMessagesByRoomQueryHandler.cs
```

O pipeline MediatR executa `LoggingBehavior` em toda request, transparentemente.

### IUserContext

Injeta o `UserId` do usuário autenticado nos handlers sem acessar JWT claims diretamente.

```csharp
internal sealed class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, Guid>
{
    private readonly Guid _userId;

    public SendMessageCommandHandler(IUserContext userContext, ...)
    {
        _userId = userContext.UserId;
    }
}
```

### IUnitOfWork

Todo write handler deve chamar `Commit` ao final para persistir as mudanças na mesma transação.

```csharp
await _messageRepository.AddAsync(message, cancellationToken);
await _unitOfWork.Commit(cancellationToken); // obrigatório
```

### Entidades de Domínio

Construtores privados, instanciação via factory methods `Create()`. Mutações que podem falhar retornam `Result`.

```csharp
// Criação via factory
var room = ChatRoom.Create(name, isPrivate, password, ownerId, createdAt);
if (room.IsFailure)
    return Result.Failure<Guid>(room.Error);

// Mutação que pode falhar
var editResult = message.Edit(newContent, utcNow);
if (editResult.IsFailure)
    return Result.Failure(editResult.Error);
```

## Real-Time (SignalR)

`IChatHub` é a abstração definida em Application (sem dependência de SignalR). `SignalRChatRoomNotifier` é a implementação concreta em Infrastructure. Os handlers de command chamam apenas a interface.

```mermaid
graph LR
    H[Handler] -->|"IChatHub (abstração)"| N[SignalRChatRoomNotifier]
    N -->|"IHubContext<ChatHub>"| Hub["ChatHub\n/chatHub"]
    Hub -->|"WebSocket"| Clients[Clientes conectados]
```

Grupos SignalR seguem a convenção `chat_{roomId}`.
