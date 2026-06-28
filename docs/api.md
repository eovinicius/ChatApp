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
