# Documentation Suite Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a complete, professional documentation suite for the ChatApp portfolio project on GitHub.

**Architecture:** README.md is the landing page with badges, overview, and links. Six focused files in `/docs/` cover architecture, API reference, database schema, configuration, and development guide. All diagrams use Mermaid (rendered natively by GitHub). Language: Portuguese.

**Tech Stack:** Markdown, Mermaid diagrams, GitHub Actions badge

## Global Constraints

- Idioma: Português em todos os arquivos
- Diagramas: Mermaid (sem imagens externas)
- GitHub username do projeto: `eovinicius` (repositório: `https://github.com/eovinicius/ChatApp`)
- Framework: .NET 10 (TargetFramework `net10.0`)
- Porta local da API: `5110`
- Nenhum arquivo de código deve ser modificado — apenas documentação

---

## Mapa de Arquivos

| Ação | Arquivo | Responsabilidade |
|------|---------|-----------------|
| Criar | `docs/superpowers/specs/2026-06-27-documentation-suite-design.md` | Rastreabilidade da decisão de design |
| Reescrever | `README.md` | Landing page: badges, overview, quickstart, links |
| Criar | `docs/architecture.md` | Camadas, padrões, fluxo de dados |
| Criar | `docs/api.md` | Referência completa de endpoints e SignalR |
| Criar | `docs/database.md` | ER diagram e schema das tabelas |
| Criar | `docs/configuration.md` | Settings, env vars, user-secrets |
| Criar | `docs/development.md` | Setup local, comandos, testes, convenções |
| Deletar | `ULTILS.md` | Substituído por `docs/development.md` |

---

## Task 1: Criar spec doc e estrutura de pastas

**Files:**
- Create: `docs/superpowers/specs/2026-06-27-documentation-suite-design.md`

- [ ] **Step 1: Criar o arquivo de spec**

Conteúdo completo do arquivo:

```markdown
---
name: documentation-suite-2026-06-27
description: Design da suite de documentação profissional para o ChatApp — portfolio público no GitHub
metadata:
  type: project
---

# ChatApp — Documentation Suite Design

**Data:** 2026-06-27  
**Status:** Aprovado

## Contexto

O ChatApp é um projeto de portfolio público no GitHub que demonstra Clean Architecture em .NET 10. A documentação existente era básica (README de 148 linhas em Português com endpoints desatualizados, ULTILS.md com nome ruim, sem referência de API, sem diagramas).

## Decisões

- **Idioma:** Português
- **Estrutura:** README.md principal + 6 arquivos em `/docs/`
- **Diagramas:** Mermaid (renderizado pelo GitHub)
- **Público:** Portfolio GitHub

## Arquivos Produzidos

| Arquivo | Propósito |
|---------|-----------|
| `README.md` | Landing page com badges, overview, quickstart, links |
| `docs/architecture.md` | Camadas Clean Architecture, padrões, fluxo de dados |
| `docs/api.md` | Referência completa de endpoints REST e SignalR |
| `docs/database.md` | ER diagram e schema PostgreSQL |
| `docs/configuration.md` | Todas as settings e variáveis de ambiente |
| `docs/development.md` | Setup local, comandos, testes, convenções de código |

## O que foi descontinuado

- `ULTILS.md` — substituído por `docs/development.md`
```

- [ ] **Step 2: Commit**

```bash
git add docs/superpowers/specs/2026-06-27-documentation-suite-design.md
git commit -m "docs: adiciona spec da suite de documentação"
```

---

## Task 2: Reescrever README.md

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Substituir o conteúdo completo do README.md**

```markdown
<div align="center">
  <h1>💬 ChatApp</h1>
  <p>API de chat em tempo real construída com .NET 10 e Clean Architecture</p>

  [![CI](https://github.com/eovinicius/ChatApp/actions/workflows/ci.yml/badge.svg)](https://github.com/eovinicius/ChatApp/actions/workflows/ci.yml)
  ![.NET](https://img.shields.io/badge/.NET-10-512BD4)
  ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)
  ![Licença](https://img.shields.io/badge/licen%C3%A7a-MIT-green)
</div>

---

ChatApp é uma API REST + WebSocket de chat em tempo real construída com .NET 10 e Clean Architecture, demonstrando CQRS, Result Pattern, domain-driven design e integração com AWS S3.

## ✨ Funcionalidades

- 🔐 **Autenticação JWT** — registro e login com tokens de curta duração
- 🏠 **Salas de chat** — crie salas públicas ou protegidas por senha (máximo 50 membros)
- 💬 **Mensagens** — envie, edite (até 1h) e delete (até 24h) mensagens de texto
- 📎 **Upload de mídia** — imagens, áudio e vídeo via AWS S3 (máximo 50 MB)
- ⚡ **Tempo real** — notificações instantâneas via SignalR WebSocket
- 🚦 **Rate limiting** — proteção contra abuso por IP e por usuário

## 🛠 Stack

| Tecnologia | Uso |
|------------|-----|
| .NET 10 / ASP.NET Core | Framework web |
| PostgreSQL | Banco de dados relacional |
| Entity Framework Core | ORM + migrações |
| MediatR | CQRS e pipeline behaviors |
| SignalR | WebSocket em tempo real |
| JWT Bearer | Autenticação |
| AWS S3 | Armazenamento de mídia |
| xUnit + NSubstitute + FluentAssertions | Testes unitários e de integração |
| Serilog | Logging estruturado com correlation ID |

## 🏛 Arquitetura

ChatApp segue o padrão **Clean Architecture** com quatro camadas. As dependências sempre apontam para o centro: `API → Application → Domain` (Infrastructure implementa interfaces de Application).

```mermaid
graph LR
    A["ChatApp.Api\nControllers · Middlewares · Hubs"] --> B["ChatApp.Application\nUse Cases · CQRS · Abstrações"]
    B --> C["ChatApp.Domain\nEntidades · Value Objects · Erros"]
    D["ChatApp.Infrastructure\nEF Core · SignalR · JWT · S3"] --> B
    D --> C
```

> Veja [docs/architecture.md](docs/architecture.md) para detalhes sobre padrões e fluxo de dados.

## 🚀 Quickstart

**Pré-requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) · [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
# 1. Clone o repositório
git clone https://github.com/eovinicius/ChatApp.git
cd ChatApp

# 2. Suba o banco de dados
docker-compose up -d --build chatapp-db

# 3. Configure as variáveis (veja docs/configuration.md)
dotnet user-secrets set "ConnectionStrings:Database" \
  "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres" \
  --project .\src\ChatApp.Api\
dotnet user-secrets set "JwtSettings:SecretKey" "dev-secret-key-min-32-characters" --project .\src\ChatApp.Api\
dotnet user-secrets set "JwtSettings:Issuer" "ChatApp" --project .\src\ChatApp.Api\
dotnet user-secrets set "JwtSettings:Audience" "ChatApp" --project .\src\ChatApp.Api\

# 4. Execute as migrações
dotnet ef database update --project .\src\ChatApp.Infrastructure\ --startup-project .\src\ChatApp.Api\

# 5. Rode a API
dotnet run --project .\src\ChatApp.Api\ChatApp.Api.csproj
```

Acesse a interface Swagger em **http://localhost:5110/swagger/index.html**

## 📖 Documentação

| Documento | Descrição |
|-----------|-----------|
| [Arquitetura](docs/architecture.md) | Camadas, padrões (Result, CQRS, IUserContext) e fluxo de dados |
| [Referência da API](docs/api.md) | Endpoints REST, autenticação, rate limiting, SignalR |
| [Banco de Dados](docs/database.md) | Modelo entidade-relacionamento e schema PostgreSQL |
| [Configuração](docs/configuration.md) | Settings, variáveis de ambiente e gerenciamento de segredos |
| [Desenvolvimento](docs/development.md) | Setup local, comandos, testes e convenções de código |

## 🧪 Testes

```bash
# Todos os testes
dotnet test

# Filtrar por classe
dotnet test --filter "FullyQualifiedName~CreateRoomTests"

# Apenas unit tests
dotnet test test/ChatApp.UnitTests/

# Apenas integration tests
dotnet test test/ChatApp.IntegrationTests/
```

## 📄 Licença

Distribuído sob a licença MIT. Veja [LICENSE](LICENSE) para detalhes.
```

- [ ] **Step 2: Verificar**

Abrir preview Markdown no VS Code e confirmar:
- Os 4 badges aparecem no header
- O diagrama Mermaid é renderizado (se o plugin Mermaid estiver instalado)
- Os links da tabela de Documentação apontam para arquivos existentes em `/docs/` (serão criados nas próximas tasks)

- [ ] **Step 3: Commit**

```bash
git add README.md
git commit -m "docs: reescreve README com badges, arquitetura e quickstart"
```

---

## Task 3: Criar docs/architecture.md

**Files:**
- Create: `docs/architecture.md`

- [ ] **Step 1: Criar o arquivo**

```markdown
# Arquitetura

## Visão Geral

ChatApp segue o padrão **Clean Architecture**, separando o sistema em quatro camadas com dependências que sempre apontam para o centro. Nenhuma camada interna conhece detalhes das camadas externas.

```mermaid
graph TD
    subgraph Externas
        API["ChatApp.Api\nControllers · Middlewares · Hubs"]
        Infra["ChatApp.Infrastructure\nEF Core · SignalR · JWT · S3"]
    end
    subgraph Internas
        App["ChatApp.Application\nUse Cases · CQRS · Abstrações"]
        Domain["ChatApp.Domain\nEntidades · Value Objects · Erros"]
    end

    API -->|"ISender (MediatR)"| App
    Infra -->|"implementa interfaces"| App
    App --> Domain
    Infra --> Domain
```

## Camadas

| Camada | Projeto | Responsabilidade |
|--------|---------|-----------------|
| **Domain** | `ChatApp.Domain` | Entidades, value objects, interfaces de repositórios. Sem dependências de framework. |
| **Application** | `ChatApp.Application` | Use cases via CQRS (MediatR). Define abstrações (`IUserRepository`, `IChatHub`, etc.) que Infrastructure implementa. |
| **Infrastructure** | `ChatApp.Infrastructure` | EF Core + PostgreSQL, SignalR, JWT, AWS S3. Implementações concretas das abstrações de Application. |
| **API** | `ChatApp.Api` | Controllers, middlewares, configuração. Despacha comandos/queries via `ISender`. |

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
    B->>B: PerformanceBehavior
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

O pipeline MediatR executa `LoggingBehavior` e `PerformanceBehavior` em toda request, transparentemente.

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
    N -->|"IHubContext&lt;ChatHub&gt;"| Hub["ChatHub\n/chatHub"]
    Hub -->|"WebSocket"| Clients[Clientes conectados]
```

Grupos SignalR seguem a convenção `chat_{roomId}`.
```

- [ ] **Step 2: Commit**

```bash
git add docs/architecture.md
git commit -m "docs: adiciona guia de arquitetura com diagramas Mermaid"
```

---

## Task 4: Criar docs/api.md

**Files:**
- Create: `docs/api.md`

- [ ] **Step 1: Criar o arquivo**

```markdown
# Referência da API

## Autenticação

A API usa **JWT Bearer tokens**. Inclua o token no header de todas as requisições autenticadas:

```http
Authorization: Bearer <token>
```

Para obter um token, faça login via `POST /api/user/login`. O token expira em **1 hora**.

## Rate Limiting

| Política | Aplica-se a | Limite | Janela | Chave de partição |
|----------|-------------|--------|--------|-------------------|
| `auth` | `/api/user/*` | 5 req | 1 min | IP do cliente |
| `chat` | `/api/chatroom/*`, `/api/message/*` | 100 req | 1 min | Username (fallback: IP) |

Excedido o limite: **HTTP 429 Too Many Requests**.

> Rate limiting é desabilitado no ambiente de testes (`Testing`).

## Formato de Resposta

**Sucesso:**
```json
{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
```

**Erro:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "ChatRoom.NotFound",
  "status": 404,
  "detail": "Sala de chat não encontrada.",
  "traceId": "00-abc123-def456-00"
}
```

O campo `traceId` corresponde ao header `X-Correlation-ID` da resposta.

---

## User

### Registrar usuário

```http
POST /api/user/register
```

**Autenticação:** Não requerida · **Rate limit:** `auth`

**Body:**
```json
{
  "name": "João Silva",
  "username": "joaosilva",
  "password": "minhasenha123"
}
```

| Campo | Tipo | Regras |
|-------|------|--------|
| `name` | `string` | Obrigatório, não vazio |
| `username` | `string` | Obrigatório, mínimo 3 caracteres, único |
| `password` | `string` | Obrigatório, mínimo 6 caracteres |

**Resposta 200:**
```json
{ "token": "<jwt>" }
```

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 400 | `User.EmptyName` | Nome não informado |
| 400 | `User.EmptyUsername` | Username não informado |
| 400 | `User.EmptyPassword` | Senha não informada |
| 409 | `User.UsernameAlreadyTaken` | Username já em uso |

---

### Login

```http
POST /api/user/login
```

**Autenticação:** Não requerida · **Rate limit:** `auth`

**Body:**
```json
{
  "username": "joaosilva",
  "password": "minhasenha123"
}
```

**Resposta 200:**
```json
{ "token": "<jwt>" }
```

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 401 | `User.InvalidCredentials` | Username ou senha inválidos |
| 404 | `User.NotFound` | Usuário não encontrado |

---

## ChatRoom

### Criar sala

```http
POST /api/chatroom
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Body:**
```json
{
  "roomName": "Sala de Desenvolvimento",
  "isPrivate": false,
  "password": null
}
```

| Campo | Tipo | Regras |
|-------|------|--------|
| `roomName` | `string` | Obrigatório, não vazio |
| `isPrivate` | `bool` | Obrigatório |
| `password` | `string?` | Obrigatório se `isPrivate = true` |

**Resposta 200:** `"<roomId>"` (Guid como string)

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 400 | `ChatRoom.EmptyName` | Nome da sala não informado |
| 400 | `ChatRoom.PrivateRoomRequiresPassword` | Sala privada precisa de senha |

---

### Entrar na sala

```http
POST /api/chatroom/{roomId}/join
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `roomId` | `Guid` | ID da sala |

**Body:**
```json
{ "password": "senha123" }
```

`password` é opcional para salas públicas; obrigatório para salas privadas.

**Resposta 200:** Sem body

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 404 | `ChatRoom.NotFound` | Sala não encontrada |
| 400 | `ChatRoom.AlreadyMember` | Usuário já é membro |
| 400 | `ChatRoom.RoomFull` | Capacidade máxima atingida (50 membros) |
| 400 | `ChatRoom.InvalidPassword` | Senha incorreta |

---

### Sair da sala

```http
DELETE /api/chatroom/{roomId}/leave
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `roomId` | `Guid` | ID da sala |

**Resposta 200:** Sem body

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 404 | `ChatRoom.NotFound` | Sala não encontrada |
| 400 | `ChatRoom.NotMember` | Usuário não é membro da sala |

---

## Message

### Listar mensagens

```http
GET /api/message?roomId={guid}&take={int}&before={datetime}
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Query parameters:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `roomId` | `Guid` | ✅ | ID da sala |
| `take` | `int` | ❌ | Mensagens por página (máximo 20, padrão 20) |
| `before` | `datetime` | ❌ | Cursor de paginação — retorna mensagens anteriores a esta data (UTC) |

**Resposta 200:**
```json
[
  {
    "content": "Olá, mundo!",
    "contentType": "text",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "sentAt": "2024-01-15T10:30:00Z"
  }
]
```

---

### Enviar mensagem

```http
POST /api/message
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Body:**
```json
{
  "roomId": "550e8400-e29b-41d4-a716-446655440000",
  "content": "Olá, mundo!",
  "contentType": "text"
}
```

| Campo | Tipo | Valores aceitos |
|-------|------|----------------|
| `roomId` | `Guid` | ID de uma sala da qual o usuário é membro |
| `content` | `string` | Texto (type `text`) ou URL do S3 (outros tipos) |
| `contentType` | `string` | `text`, `image`, `audio`, `video` |

**Resposta 200:** `"<messageId>"` (Guid como string)

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 404 | `ChatRoom.NotFound` | Sala não encontrada |
| 400 | `ChatRoom.NotMember` | Usuário não é membro da sala |
| 400 | `ChatMessage.EmptyContent` | Conteúdo da mensagem vazio |

---

### Editar mensagem

```http
PUT /api/message/{messageId}
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `messageId` | `Guid` | ID da mensagem |

**Body:**
```json
{
  "roomId": "550e8400-e29b-41d4-a716-446655440000",
  "content": "Texto corrigido"
}
```

**Restrições:** Apenas mensagens de tipo `text`. Janela de edição: **1 hora** após o envio.

**Resposta 200:** Sem body

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 404 | `ChatMessage.NotFound` | Mensagem não encontrada |
| 403 | `ChatMessage.Unauthorized` | Não é o autor da mensagem |
| 400 | `ChatMessage.EditWindowExpired` | Janela de 1h encerrada |
| 400 | `ChatMessage.NotTextMessage` | Só mensagens de texto podem ser editadas |

---

### Deletar mensagem

```http
DELETE /api/message/{messageId}?roomId={guid}
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

| Parâmetro | Local | Tipo | Descrição |
|-----------|-------|------|-----------|
| `messageId` | rota | `Guid` | ID da mensagem |
| `roomId` | query | `Guid` | ID da sala |

**Restrições:** Janela de exclusão: **24 horas** após o envio.

**Resposta 200:** Sem body

**Erros:**

| HTTP | Error Code | Descrição |
|------|------------|-----------|
| 404 | `ChatMessage.NotFound` | Mensagem não encontrada |
| 403 | `ChatMessage.Unauthorized` | Não é o autor ou não é membro da sala |

---

### Upload de mídia

```http
POST /api/message/upload
```

**Autenticação:** ✅ JWT · **Rate limit:** `chat`

**Body:** `multipart/form-data`

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `file` | `IFormFile` | Arquivo de mídia |

**Tipos MIME aceitos:** `image/*`, `audio/*`, `video/*`, `application/pdf`

**Extensões aceitas:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.mp4`, `.mov`, `.webm`, `.mp3`, `.ogg`, `.wav`, `.m4a`, `.pdf`

**Tamanho máximo:** 50 MB

**Resposta 200:** URL assinada do S3 (válida por 10 minutos)

---

## SignalR

### Conexão

Endpoint WebSocket: `/chatHub`

Requer autenticação JWT:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5110/chatHub", {
    accessTokenFactory: () => localStorage.getItem("token")
  })
  .build();

await connection.start();
```

### Métodos invocados pelo cliente

| Método | Parâmetros | Descrição |
|--------|------------|-----------|
| `JoinRoom` | `roomId: string` | Entra no grupo SignalR da sala |
| `LeaveRoom` | `roomId: string` | Sai do grupo SignalR da sala |
| `SendMessage` | `roomId: string, message: string` | Envia mensagem via WebSocket |

### Eventos recebidos pelo cliente

| Evento | Parâmetros | Quando ocorre |
|--------|------------|---------------|
| `JoinGroup` | `roomId: string, username: string` | Um usuário entra na sala |
| `LeftGroup` | `roomId: string, username: string` | Um usuário sai da sala |
| `SendMessageToGroup` | `roomId: string, message: string` | Uma nova mensagem foi enviada na sala |

**Grupos:** Cada sala tem um grupo SignalR com o padrão `chat_{roomId}`.

```javascript
// Exemplo completo de uso
connection.on("SendMessageToGroup", (roomId, message) => {
  console.log(`[${roomId}] Nova mensagem: ${message}`);
});

await connection.invoke("JoinRoom", roomId);
await connection.invoke("SendMessage", roomId, "Olá!");
```
```

- [ ] **Step 2: Commit**

```bash
git add docs/api.md
git commit -m "docs: adiciona referência completa da API REST e SignalR"
```

---

## Task 5: Criar docs/database.md

**Files:**
- Create: `docs/database.md`

- [ ] **Step 1: Criar o arquivo**

```markdown
# Banco de Dados

## Modelo Entidade-Relacionamento

```mermaid
erDiagram
    users {
        uuid id PK
        varchar name
        varchar username UK
        text password
    }

    chat_rooms {
        uuid id PK
        varchar name
        varchar password
        uuid owner_id FK
        boolean is_private
        timestamptz created_at
        int max_members
    }

    members {
        uuid chat_room_id PK,FK
        uuid user_id PK,FK
        timestamptz joined_at
        boolean is_admin
    }

    messages {
        uuid id PK
        uuid chat_room_id FK
        text content
        uuid sender_id FK
        timestamptz sent_at
        timestamptz edited_at
        text content_type
    }

    users ||--o{ members : "participa de"
    chat_rooms ||--o{ members : "possui"
    users ||--o{ messages : "envia"
    chat_rooms ||--o{ messages : "contém"
    users ||--o{ chat_rooms : "cria"
```

## Tabelas

### `users`

| Coluna | Tipo | Constraints | Descrição |
|--------|------|-------------|-----------|
| `id` | `uuid` | PK | Identificador único |
| `name` | `varchar(100)` | NOT NULL | Nome de exibição |
| `username` | `varchar(200)` | NOT NULL, UNIQUE | Nome de login |
| `password` | `text` | NOT NULL | Hash BCrypt da senha |

---

### `chat_rooms`

| Coluna | Tipo | Constraints | Descrição |
|--------|------|-------------|-----------|
| `id` | `uuid` | PK | Identificador único |
| `name` | `varchar(50)` | NOT NULL | Nome da sala |
| `password` | `varchar(30)` | — | Hash da senha (vazio se sala pública) |
| `owner_id` | `uuid` | NOT NULL | ID do usuário criador |
| `is_private` | `boolean` | NOT NULL | Se a sala requer senha para entrar |
| `created_at` | `timestamptz` | NOT NULL | Data de criação (UTC) |
| `max_members` | `int` | NOT NULL | Capacidade máxima (fixo em 50) |

---

### `members`

Tabela de junção entre `users` e `chat_rooms` — registra quem pertence a qual sala.

| Coluna | Tipo | Constraints | Descrição |
|--------|------|-------------|-----------|
| `chat_room_id` | `uuid` | PK, FK → `chat_rooms.id` | ID da sala |
| `user_id` | `uuid` | PK, FK → `users.id` | ID do usuário |
| `joined_at` | `timestamptz` | NOT NULL | Data de entrada na sala (UTC) |
| `is_admin` | `boolean` | NOT NULL | Se o usuário é administrador da sala |

**Cascade delete:** Ao deletar um `chat_room` ou um `user`, todos os registros de `members` correspondentes são excluídos automaticamente.

---

### `messages`

| Coluna | Tipo | Constraints | Descrição |
|--------|------|-------------|-----------|
| `id` | `uuid` | PK | Identificador único |
| `chat_room_id` | `uuid` | FK → `chat_rooms.id` | Sala onde a mensagem foi enviada |
| `content` | `text` | NOT NULL | Conteúdo textual ou URL do arquivo S3 |
| `sender_id` | `uuid` | FK → `users.id` | Autor da mensagem |
| `sent_at` | `timestamptz` | NOT NULL | Data de envio (UTC) |
| `edited_at` | `timestamptz` | — | Data da última edição (`null` se não editada) |
| `content_type` | `text` | NOT NULL | `text`, `image`, `audio` ou `video` |

## Índices

| Índice | Tabela | Coluna(s) | Tipo | Motivo |
|--------|--------|-----------|------|--------|
| `ix_users_username` | `users` | `username` | UNIQUE | Busca de login e garantia de unicidade |
| `ix_messages_chat_room_id` | `messages` | `chat_room_id` | Normal | Filtragem de mensagens por sala |
| `ix_messages_sent_at` | `messages` | `sent_at` | Normal | Paginação por cursor de data (query `before`) |

## Migrações

As migrações são gerenciadas pelo Entity Framework Core e ficam em `src/ChatApp.Infrastructure/Migrations/`. Em ambiente `Development`, são aplicadas automaticamente na inicialização da API via `app.ApplyMigrations()` em `Program.cs`.
```

- [ ] **Step 2: Commit**

```bash
git add docs/database.md
git commit -m "docs: adiciona modelo de dados com ER diagram Mermaid"
```

---

## Task 6: Criar docs/configuration.md

**Files:**
- Create: `docs/configuration.md`

- [ ] **Step 1: Criar o arquivo**

```markdown
# Configuração

## Estrutura do appsettings.json

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "<sua-chave-secreta-min-32-caracteres>",
    "Issuer": "ChatApp",
    "Audience": "ChatApp"
  },
  "AwsSettings": {
    "S3": {
      "BucketName": "seu-bucket-s3",
      "Region": "us-east-1",
      "AccessKey": "",
      "SecretKey": ""
    }
  }
}
```

## Referência Completa

### ConnectionStrings

| Chave | Obrigatório | Exemplo | Descrição |
|-------|-------------|---------|-----------|
| `ConnectionStrings:Database` | ✅ | `Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres` | Connection string PostgreSQL no formato Npgsql |

---

### JwtSettings

| Chave | Obrigatório | Exemplo | Descrição |
|-------|-------------|---------|-----------|
| `JwtSettings:SecretKey` | ✅ | `minha-chave-secreta-muito-longa` | Chave HMAC-SHA256. Recomendado: mínimo 32 caracteres |
| `JwtSettings:Issuer` | ✅ | `ChatApp` | Emissor do token JWT (`iss` claim) |
| `JwtSettings:Audience` | ✅ | `ChatApp` | Audiência do token JWT (`aud` claim) |

---

### AwsSettings

| Chave | Obrigatório | Exemplo | Descrição |
|-------|-------------|---------|-----------|
| `AwsSettings:S3:BucketName` | ✅ | `meu-bucket-chatapp` | Nome do bucket S3 para upload de mídia |
| `AwsSettings:S3:Region` | ✅ | `us-east-1` | Região AWS onde o bucket está localizado |
| `AwsSettings:S3:AccessKey` | ❌ | `AKIAIOSFODNN7EXAMPLE` | Access Key ID. Omita se usar IAM Role |
| `AwsSettings:S3:SecretKey` | ❌ | `wJalrXUtnFEMI/K7MDENG` | Secret Access Key. Omita se usar IAM Role |

> Se `AccessKey` e `SecretKey` forem omitidos, o AWS SDK usa as credenciais padrão do ambiente (IAM Role, variável de ambiente `AWS_*`, etc.).

---

## Desenvolvimento Local com User Secrets

**Nunca commite credenciais no repositório.** Use `dotnet user-secrets` em desenvolvimento:

```bash
# Habilitar user secrets no projeto (executar uma vez)
dotnet user-secrets init --project .\src\ChatApp.Api\

# Configurar as secrets
dotnet user-secrets set "ConnectionStrings:Database" \
  "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres" \
  --project .\src\ChatApp.Api\

dotnet user-secrets set "JwtSettings:SecretKey" "dev-secret-key-at-least-32-characters!!" \
  --project .\src\ChatApp.Api\

dotnet user-secrets set "JwtSettings:Issuer" "ChatApp" --project .\src\ChatApp.Api\
dotnet user-secrets set "JwtSettings:Audience" "ChatApp" --project .\src\ChatApp.Api\

# Para funcionalidade de upload de arquivo (opcional em dev):
dotnet user-secrets set "AwsSettings:S3:BucketName" "meu-bucket-dev" --project .\src\ChatApp.Api\
dotnet user-secrets set "AwsSettings:S3:Region" "us-east-1" --project .\src\ChatApp.Api\
dotnet user-secrets set "AwsSettings:S3:AccessKey" "sua-access-key" --project .\src\ChatApp.Api\
dotnet user-secrets set "AwsSettings:S3:SecretKey" "sua-secret-key" --project .\src\ChatApp.Api\
```

As user secrets sobrescrevem `appsettings.json` apenas em ambiente `Development`.

---

## Variáveis de Ambiente (Produção)

Em produção, sobrescreva qualquer configuração via variáveis de ambiente. O .NET usa `__` como separador de hierarquia:

| Variável de Ambiente | Equivalente no appsettings |
|---------------------|---------------------------|
| `ConnectionStrings__Database` | `ConnectionStrings.Database` |
| `JwtSettings__SecretKey` | `JwtSettings.SecretKey` |
| `JwtSettings__Issuer` | `JwtSettings.Issuer` |
| `JwtSettings__Audience` | `JwtSettings.Audience` |
| `AwsSettings__S3__BucketName` | `AwsSettings.S3.BucketName` |
| `AwsSettings__S3__Region` | `AwsSettings.S3.Region` |
| `AwsSettings__S3__AccessKey` | `AwsSettings.S3.AccessKey` |
| `AwsSettings__S3__SecretKey` | `AwsSettings.S3.SecretKey` |

---

## CORS

Por padrão, a API permite requisições das seguintes origens:

| Origem | Framework típico |
|--------|-----------------|
| `http://localhost:3000` | React CRA, Vue CLI |
| `http://localhost:5173` | Vite (React, Vue, Svelte) |
| `http://localhost:4200` | Angular CLI |

Para adicionar outras origens, edite a política de CORS em `src/ChatApp.Api/Extensions/ApplicationBuilderExtensions.cs`.
```

- [ ] **Step 2: Commit**

```bash
git add docs/configuration.md
git commit -m "docs: adiciona referência de configuração e gerenciamento de segredos"
```

---

## Task 7: Criar docs/development.md

**Files:**
- Create: `docs/development.md`

- [ ] **Step 1: Criar o arquivo**

```markdown
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
```

- [ ] **Step 2: Commit**

```bash
git add docs/development.md
git commit -m "docs: adiciona guia de desenvolvimento, testes e convenções de código"
```

---

## Task 8: Cleanup e commit final

**Files:**
- Delete: `ULTILS.md`

- [ ] **Step 1: Deletar ULTILS.md**

```bash
git rm ULTILS.md
```

- [ ] **Step 2: Verificação final**

Checar que todos os links do README apontam para arquivos existentes:

```bash
# Verificar que todos os docs foram criados
ls docs/
# Esperado: architecture.md  api.md  database.md  configuration.md  development.md  superpowers/
```

Verificar que `dotnet build` ainda passa:

```bash
dotnet build
# Esperado: Build succeeded.
```

- [ ] **Step 3: Commit final**

```bash
git add -A
git commit -m "docs: remove ULTILS.md substituído por docs/development.md"
```

---

## Self-Review

### Cobertura do Spec

| Requisito | Task |
|-----------|------|
| README com badges, overview, quickstart, links | Task 2 |
| docs/architecture.md com diagramas Mermaid e padrões | Task 3 |
| docs/api.md com todos os endpoints e SignalR | Task 4 |
| docs/database.md com ER diagram | Task 5 |
| docs/configuration.md com todas as settings | Task 6 |
| docs/development.md com setup, comandos, convenções | Task 7 |
| Deletar ULTILS.md | Task 8 |
| Spec doc de rastreabilidade | Task 1 |

✅ Todas as seções do spec têm uma task correspondente.

### Placeholder Scan

✅ Nenhum TBD, TODO, ou "implement later" no plano. Todo conteúdo dos arquivos está completo e pronto para copiar.

### Consistência

✅ `.NET 10` usado em todos os arquivos (baseado em `TargetFramework: net10.0` no `.csproj`).  
✅ `eovinicius` é o GitHub username (baseado na URL do README atual).  
✅ Porta `5110` consistente com ULTILS.md e CLAUDE.md.  
✅ Todos os endpoints na Task 4 batem com os controllers encontrados na exploração.
