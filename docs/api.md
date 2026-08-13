# Referência da API

Todas as rotas são versionadas: `/api/v{version}/...`. A versão atual é **v1** e é assumida quando omitida.

## Autenticação

A API usa **JWT Bearer tokens**. Inclua o token no header de todas as requisições autenticadas:

```http
Authorization: Bearer <token>
```

Para obter um token: `POST /api/v1/auth/register` ou `POST /api/v1/auth/login`.

No WebSocket o header não é possível — o token vai na query string (`?access_token=<jwt>`), que é o que o cliente SignalR do browser faz automaticamente via `accessTokenFactory`.

## Formato de resposta

**Toda resposta com corpo** usa o mesmo envelope. `data` e `error` estão sempre presentes, e exatamente um deles é não-nulo. `meta` só aparece quando há algo a dizer.

### Sucesso — 200 / 201

```json
{
  "data": { "id": "550e8400-e29b-41d4-a716-446655440000" },
  "error": null
}
```

### Lista paginada — 200

```json
{
  "data": [ /* itens */ ],
  "error": null,
  "meta": {
    "pagination": {
      "nextCursor": "2026-08-13T10:22:31.482Z",
      "hasMore": true
    }
  }
}
```

`nextCursor` é o valor que deve ser reenviado como `?before=` para buscar a página seguinte. Quando `hasMore` é `false`, acabou.

### Comando sem retorno — 204

Sem corpo. É a única resposta de sucesso que não traz envelope.

### Erro — status semântico, `application/json`

```json
{
  "data": null,
  "error": {
    "code": "Conversation.NotFound",
    "message": "Conversa não encontrada.",
    "type": "NotFound",
    "traceId": "00-a1b2c3d4e5f60718293a4b5c6d7e8f90-1a2b3c4d5e6f7081-01"
  }
}
```

| Campo | Uso |
|-------|-----|
| `code` | **Identificador estável.** É nele que o cliente deve ramificar. |
| `message` | Texto em pt-BR, apresentável ao usuário final. |
| `type` | Categoria (`Validation`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `Failure`) — determina o status HTTP. |
| `details` | Presente apenas em falhas de validação de campo (ver abaixo). |
| `traceId` | Id da Activity da requisição, para correlacionar com os logs. O header `X-Correlation-ID` também é devolvido em toda resposta. |

### Erro de validação — 400

Falhas de campo vêm em `details`, com o mesmo envelope:

```json
{
  "data": null,
  "error": {
    "code": "Validation.Failed",
    "message": "Um ou mais campos são inválidos.",
    "type": "Validation",
    "details": [
      { "field": "Username", "message": "O username deve ter no mínimo 3 caracteres" }
    ],
    "traceId": "00-a1b2…-01"
  }
}
```

### Mapa de status

| `type` | HTTP |
|--------|------|
| `Validation` | 400 |
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `NotFound` | 404 |
| `Conflict` | 409 |
| `Failure` | 500 |

Erros produzidos pelo próprio pipeline HTTP (sem passar por um endpoint) usam o mesmo envelope:

| HTTP | `code` |
|------|--------|
| 401 | `Auth.Unauthorized` |
| 403 | `Auth.Forbidden` |
| 404 (rota inexistente) | `Http.NotFound` |
| 405 | `Http.MethodNotAllowed` |
| 415 | `Http.UnsupportedMediaType` |
| 429 | `Http.TooManyRequests` |
| 400 (JSON malformado) | `Http.MalformedRequest` |
| 500 | `Server.Unexpected` |

## Rate limiting

| Política | Aplica-se a | Chave de partição |
|----------|-------------|-------------------|
| `auth` | `/api/v1/auth/*` | IP do cliente |
| `chat` | `/api/v1/conversations/*`, `/api/v1/messages/*`, `/api/v1/users/*` | Username (fallback: IP) |

Excedido o limite: **429** com `code: "Http.TooManyRequests"`.

> Rate limiting é desabilitado no ambiente `Development`.

---

## Auth

### Registrar usuário

```http
POST /api/v1/auth/register
```

**Autenticação:** não requerida · **Rate limit:** `auth`

```json
{ "name": "João Silva", "username": "joaosilva", "password": "minhasenha123" }
```

| Campo | Regras |
|-------|--------|
| `name` | Obrigatório |
| `username` | Obrigatório, mínimo 3 caracteres, único |
| `password` | Obrigatório, mínimo 6 caracteres |

**200:** `{ "data": { "token": "<jwt>" }, "error": null }`

| HTTP | `code` |
|------|--------|
| 400 | `Validation.Failed`, `User.EmptyName`, `User.EmptyUsername`, `User.EmptyPassword` |
| 409 | `User.UsernameAlreadyTaken` |

### Login

```http
POST /api/v1/auth/login
```

**Autenticação:** não requerida · **Rate limit:** `auth`

```json
{ "username": "joaosilva", "password": "minhasenha123" }
```

**200:** `{ "data": { "token": "<jwt>" }, "error": null }`

| HTTP | `code` |
|------|--------|
| 401 | `User.InvalidCredentials` |
| 404 | `User.NotFound` |

---

## Users

### Buscar usuários

```http
GET /api/v1/users?search={texto}&take={int}
```

Ponto de partida de uma conversa 1x1. Não inclui o próprio usuário. Não paginado — sem `meta`.

**200:** `data` é um array de `{ id, name, username, avatarUrl, lastSeenAt }`.

### Usuário autenticado

```http
GET /api/v1/users/me
```

**200:** `data` é o próprio usuário.

---

## Conversations

Uma conversa tem `type` **`direct`** (1x1) ou **`group`**. O tipo governa os invariantes: conversas diretas têm exatamente dois participantes, membros imutáveis, sem nome e sem admins.

### Abrir conversa 1x1 (idempotente)

```http
POST /api/v1/conversations/direct
```

```json
{ "targetUserId": "550e8400-e29b-41d4-a716-446655440000" }
```

Chamar duas vezes com o mesmo alvo devolve **a mesma conversa** — garantido pelo `DirectKey` com índice único.

**201** + header `Location`: `{ "data": { "id": "<conversationId>" }, "error": null }`

| HTTP | `code` |
|------|--------|
| 400 | `Conversation.DirectWithSelf` |
| 404 | `Conversation.TargetUserNotFound` |

### Criar grupo

```http
POST /api/v1/conversations/group
```

```json
{ "name": "Time de Produto", "memberIds": ["<guid>", "<guid>"] }
```

Quem cria vira **owner**. **201** + `Location`: `{ "data": { "id": "<conversationId>" } }`

| HTTP | `code` |
|------|--------|
| 400 | `Conversation.EmptyGroupName`, `Conversation.MemberNotFound` |
| 409 | `Conversation.Full` |

### Listar conversas (tela inicial)

```http
GET /api/v1/conversations?before={datetime}&take={int}
```

Ordenadas pela última atividade, com prévia da última mensagem e contagem de não-lidas. **Paginada** — traz `meta.pagination`. `take` padrão 30, máximo 100.

**200:**
```json
{
  "data": [
    {
      "id": "<guid>",
      "type": "direct",
      "title": "João Silva",
      "avatarUrl": null,
      "otherUserId": "<guid>",
      "lastActivityAt": "2026-08-13T10:22:31Z",
      "unreadCount": 3,
      "lastMessage": {
        "id": "<guid>",
        "content": "até amanhã",
        "contentType": "text",
        "senderId": "<guid>",
        "sentAt": "2026-08-13T10:22:31Z",
        "isDeleted": false
      }
    }
  ],
  "error": null,
  "meta": { "pagination": { "nextCursor": "2026-08-13T10:22:31Z", "hasMore": true } }
}
```

Em conversas `direct`, `title` e `avatarUrl` são hidratados a partir do outro participante.

### Detalhe da conversa

```http
GET /api/v1/conversations/{conversationId}
```

**200:** `data` traz `type`, `title`, `ownerId` e `participants`. Não paginado.

| HTTP | `code` |
|------|--------|
| 403 | `Conversation.NotParticipant` |
| 404 | `Conversation.NotFound` |

### Renomear grupo

```http
PATCH /api/v1/conversations/{conversationId}
```

```json
{ "name": "Novo nome" }
```

**204.** Requer admin.

| HTTP | `code` |
|------|--------|
| 403 | `Conversation.RequiresAdmin` |
| 409 | `Conversation.DirectIsImmutable` |

### Participantes

```http
POST   /api/v1/conversations/{conversationId}/participants                    { userIds[] }
DELETE /api/v1/conversations/{conversationId}/participants/me
DELETE /api/v1/conversations/{conversationId}/participants/{userId}
POST   /api/v1/conversations/{conversationId}/participants/{userId}/promote
POST   /api/v1/conversations/{conversationId}/participants/{userId}/demote
```

Todas respondem **204**. Adicionar, remover, promover e rebaixar exigem admin; o **owner não pode sair sem transferir a propriedade antes**.

| HTTP | `code` |
|------|--------|
| 400 | `Conversation.MemberNotFound` |
| 403 | `Conversation.RequiresAdmin`, `Conversation.RequiresOwner`, `Conversation.CannotRemoveOwner`, `Conversation.NotParticipant` |
| 404 | `Conversation.NotFound`, `Conversation.ParticipantNotFound` |
| 409 | `Conversation.AlreadyParticipant`, `Conversation.Full`, `Conversation.DirectIsImmutable`, `Conversation.OwnerMustTransferFirst` |

### Marcar como lida

```http
POST /api/v1/conversations/{conversationId}/read
```

```json
{ "lastMessageId": "<guid>" }
```

Zera as não-lidas e dispara o recibo de leitura em tempo real. O cursor é **monotônico**: um ack fora de ordem nunca o move para trás. **204.**

---

## Messages

### Listar mensagens

```http
GET /api/v1/conversations/{conversationId}/messages?before={datetime}&take={int}
```

Mais recentes primeiro. **Paginada** — traz `meta.pagination`. `take` padrão 30, máximo 100.

**200:** cada item traz `id`, `senderId`, `content`, `contentType`, `fileName`, `sizeBytes`, `sentAt`, `editedAt`, `isEdited`, `isDeleted`, `status` e `readByCount`.

`status` reflete os recibos: `sent` (✓), `delivered` (✓✓), `read` (✓✓ azul).

Mensagens apagadas continuam na lista com `isDeleted: true` e o texto substituído por *"Esta mensagem foi apagada"* — o apagamento é lógico, para não abrir buracos entre o cursor de leitura e a contagem de não-lidas.

### Enviar mensagem

```http
POST /api/v1/conversations/{conversationId}/messages
```

```json
{
  "content": "Olá!",
  "contentType": "text",
  "storageKey": null,
  "fileName": null,
  "sizeBytes": null
}
```

| Campo | Valores |
|-------|---------|
| `contentType` | `text`, `image`, `audio`, `video` |
| `content` | Texto (`text`) ou a URL devolvida pelo upload (demais tipos) |
| `storageKey` | Obrigatório para mídia — é a chave do S3, necessária para apagar depois |

**201:** `{ "data": { "id": "<messageId>" }, "error": null }` (sem `Location`: não há rota GET para uma mensagem isolada).

| HTTP | `code` |
|------|--------|
| 400 | `Message.EmptyContent`, `Message.InvalidContentType`, `Message.MissingStorageKey` |
| 403 | `Conversation.NotParticipant` |
| 404 | `Conversation.NotFound` |

### Editar mensagem

```http
PUT /api/v1/messages/{messageId}
```

```json
{ "content": "Texto corrigido" }
```

Apenas mensagens de texto, dentro da janela de edição. **204.**

| HTTP | `code` |
|------|--------|
| 400 | `Message.EmptyContent` |
| 403 | `Message.Unauthorized` |
| 404 | `Message.NotFound` |
| 409 | `Message.EditWindowExpired`, `Message.NotTextMessage`, `Message.AlreadyDeleted` |

### Apagar mensagem

```http
DELETE /api/v1/messages/{messageId}
```

Apagamento lógico, dentro da janela de exclusão. **204.**

| HTTP | `code` |
|------|--------|
| 403 | `Message.Unauthorized` |
| 404 | `Message.NotFound` |
| 409 | `Message.DeleteWindowExpired`, `Message.AlreadyDeleted` |

### Upload de mídia

```http
POST /api/v1/messages/upload
```

**Body:** `multipart/form-data` com o campo `file`.

**Tipos aceitos:** `image/*`, `audio/*`, `video/*`, `application/pdf`
**Extensões:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.mp4`, `.mov`, `.webm`, `.mp3`, `.ogg`, `.wav`, `.m4a`, `.pdf`
**Tamanho máximo:** 50 MB

**200:**
```json
{
  "data": {
    "url": "<presigned-url>",
    "storageKey": "<s3-key>",
    "fileName": "foto.jpg",
    "sizeBytes": 184320
  },
  "error": null
}
```

O cliente devolve esses campos em `POST /conversations/{id}/messages`.

| HTTP | `code` |
|------|--------|
| 400 | `UploadFile.EmptyFile`, `UploadFile.FileTooLarge`, `UploadFile.InvalidContentType`, `UploadFile.InvalidExtension` |

---

## SignalR

Endpoint: `/chatHub`. **Escritas nunca passam pelo hub** — enviar mensagem é HTTP; o hub só notifica.

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/chatHub", {
    accessTokenFactory: () => localStorage.getItem("token")
  })
  .build();

await connection.start();
```

A entrega é **endereçada por usuário**, não por grupo: não há bookkeeping de grupos, sobrevive a reconexões e alcança todos os dispositivos do usuário.

### Invocado pelo cliente

| Método | Parâmetros |
|--------|------------|
| `Typing` | `conversationId: string, isTyping: bool` |

### Recebido pelo cliente

Os nomes canônicos dos eventos estão em `IChatClient` (`Chat.Infrastructure`). As respostas do hub **não** usam o envelope — ele é um contrato de mensagens, não HTTP.

> As mensagens do hub são notificações; o estado de verdade vem sempre dos endpoints HTTP.
