# Guia de Desenvolvimento

## Pré-requisitos

| Ferramenta | Versão mínima | Download |
|------------|---------------|----------|
| .NET SDK | 10.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Docker Desktop | Qualquer recente | [docker.com](https://www.docker.com/products/docker-desktop/) |

## Setup Local

### 1. Clone e entre no diretório

```bash
git clone https://github.com/eovinicius/ChatApp.git
cd ChatApp
```

### 2. Suba o banco de dados

```bash
docker-compose up -d --build chatapp-db
```

Aguarde o PostgreSQL subir. Verifique com `docker-compose ps`.

### 3. Configure as secrets

```bash
dotnet user-secrets init --project .\src\ChatApp.Api\

dotnet user-secrets set "ConnectionStrings:Database" \
  "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres" \
  --project .\src\ChatApp.Api\

dotnet user-secrets set "JwtSettings:SecretKey" "dev-secret-key-at-least-32-characters!!" \
  --project .\src\ChatApp.Api\

dotnet user-secrets set "JwtSettings:Issuer" "ChatApp" --project .\src\ChatApp.Api\
dotnet user-secrets set "JwtSettings:Audience" "ChatApp" --project .\src\ChatApp.Api\
```

> Para funcionalidade de upload de mídia, configure também `AwsSettings:S3:*`. Veja [configuration.md](configuration.md).

### 4. Execute as migrações

```bash
dotnet ef database update \
  --project .\src\ChatApp.Infrastructure\ \
  --startup-project .\src\ChatApp.Api\
```

### 5. Rode a API

```bash
dotnet run --project .\src\ChatApp.Api\ChatApp.Api.csproj
```

Swagger disponível em **http://localhost:5110/swagger/index.html**

---

## Comandos de Referência

```bash
# Restaurar dependências
dotnet restore

# Build
dotnet build

# Rodar todos os testes
dotnet test

# Filtrar testes por classe
dotnet test --filter "FullyQualifiedName~CreateRoomTests"

# Apenas unit tests
dotnet test test/ChatApp.UnitTests/

# Apenas integration tests
dotnet test test/ChatApp.IntegrationTests/

# Adicionar nova migration
dotnet ef migrations add <NomeDaMigration> \
  --project .\src\ChatApp.Infrastructure\ \
  --startup-project .\src\ChatApp.Api\

# Aplicar migrations pendentes
dotnet ef database update \
  --project .\src\ChatApp.Infrastructure\ \
  --startup-project .\src\ChatApp.Api\

# Remover última migration (apenas se não aplicada)
dotnet ef migrations remove \
  --project .\src\ChatApp.Infrastructure\ \
  --startup-project .\src\ChatApp.Api\

# Subir apenas o banco
docker-compose up -d --build chatapp-db

# Limpar containers e volumes do Docker
docker system prune -a --volumes
```

---

## Testes

### Stack

| Biblioteca | Uso |
|-----------|-----|
| **xUnit** | Framework de testes |
| **NSubstitute** | Mocking de dependências |
| **FluentAssertions** | Assertions legíveis |
| **WebApplicationFactory** | Integration tests com servidor real |

### Unit Tests (`test/ChatApp.UnitTests/`)

Nenhum container de DI — todas as dependências são mockadas via `NSubstitute.Substitute.For<T>()`. Os handlers são instanciados diretamente.

```csharp
// Padrão dos unit tests
[Fact]
public async Task DeveCriarSalaDeChatComSucesso()
{
    // Arrange
    var roomRepository = Substitute.For<IChatRoomRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    var userContext = Substitute.For<IUserContext>();
    userContext.UserId.Returns(Guid.NewGuid());

    var handler = new CreateRoomCommandHandler(roomRepository, unitOfWork, userContext, ...);
    var command = new CreateRoomCommand("Sala Geral", false, null);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    await roomRepository.Received(1).AddAsync(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
}
```

### Integration Tests (`test/ChatApp.IntegrationTests/`)

Usam `WebApplicationFactory` com PostgreSQL real. Herdam de `IntegrationTestBase`.

**Convenção:** Nomes de teste em **Português** — `DeveCriarSalaDeChatComSucesso`, `DeveRetornarErroQuandoNomeVazio`.

---

## Criando um Novo Use Case

Siga o padrão estabelecido no projeto:

### 1. Crie a pasta e os arquivos

```
src/ChatApp.Application/UseCases/{Feature}/{UseCaseName}/
├── {UseCaseName}Command.cs        ← ou Query.cs
└── {UseCaseName}CommandHandler.cs ← ou QueryHandler.cs
```

### 2. Implemente o command ou query

```csharp
// Command (altera estado)
public sealed record CreateRoomCommand(
    string Name,
    bool IsPrivate,
    string? Password) : ICommand<Guid>;

// Query (apenas lê)
public sealed record GetMessagesByRoomQuery(
    Guid RoomId,
    int Take,
    DateTime? Before) : IQuery<IReadOnlyList<GetMessagesByRoomResponse>>;
```

### 3. Implemente o handler

```csharp
internal sealed class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand, Guid>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Guid _userId;

    public CreateRoomCommandHandler(
        IChatRoomRepository roomRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
        _userId = userContext.UserId;
    }

    public async Task<Result<Guid>> Handle(CreateRoomCommand command, CancellationToken ct)
    {
        var room = ChatRoom.Create(command.Name, command.IsPrivate, command.Password, _userId, DateTime.UtcNow);
        if (room.IsFailure)
            return Result.Failure<Guid>(room.Error);

        await _roomRepository.AddAsync(room.Value, ct);
        await _unitOfWork.Commit(ct);  // sempre obrigatório em write handlers

        return room.Value.Id;
    }
}
```

### 4. Adicione o endpoint no controller

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateChatRoomRequest request)
{
    var command = new CreateRoomCommand(request.RoomName, request.IsPrivate, request.Password);
    var result = await _sender.Send(command);
    return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
}
```

---

## Convenções de Código

| Convenção | Regra |
|-----------|-------|
| **Nomes de teste** | Em Português: `DeveCriarMensagemComSucesso` |
| **Injeção de dependência** | Sempre via construtor, nunca via `ServiceLocator` |
| **Write handlers** | Sempre chamar `_unitOfWork.Commit(ct)` no final |
| **Domain errors** | Definir em classe estática `*Errors.cs` ao lado da entidade |
| **Handlers** | `internal sealed class` — não são públicos fora de Application |
| **Erros de negócio** | Nunca lançar exceção; use `Result.Failure(error)` |
| **Factories** | Entidades instanciadas apenas via método estático `Create()` |
