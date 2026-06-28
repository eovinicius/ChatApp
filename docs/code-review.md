# Code Review: ChatApp — Análise como Tech Lead Sênior

## Contexto

Análise completa do projeto ChatApp (.NET 10, Clean Architecture) cobrindo Domain, Application, Infrastructure, API e testes. Objetivo: identificar bugs, falhas de segurança, violações de design, problemas de qualidade e oportunidades de melhoria, com priorização por impacto e esforço.

---

## Visão Geral da Arquitetura

**Stack:** .NET 10 · ASP.NET Core · EF Core + PostgreSQL · MediatR (CQRS) · SignalR · JWT · AWS S3 · Serilog · xUnit + NSubstitute + FluentAssertions

**Estrutura de camadas (Clean Architecture):**
```
Domain → Application → Infrastructure → API
```
A direção de dependências está correta. A injeção de dependência está bem estruturada. O padrão Result<T> e CQRS via MediatR são pontos positivos. Os problemas estão nos detalhes de implementação, não na arquitetura geral.

---

## PROBLEMAS ENCONTRADOS

---

### 🔴 CRÍTICOS — Devem ser corrigidos imediatamente

---

#### C1. SendMessageCommandHandler retorna Guid aleatório em vez do ID da mensagem
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/SendMessage/SendMessageCommandHandler.cs:69`
- **Problema:** `return Result.Success(Guid.NewGuid())` cria um GUID novo em vez de retornar `chatMessage.Id`. O cliente nunca sabe qual mensagem foi criada.
- **Impacto:** Quebraria qualquer feature de rastreamento de mensagens no frontend.
- **Fix:** `return Result.Success(chatMessage.Id);`
- **Obs. de teste:** `SendMessageTests.cs` verifica apenas `IsSuccess`, não o valor retornado — o teste não captura esse bug.

---

#### C2. GetMessagesByRoomQueryHandler sem controle de acesso
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/GetMessagesByRoom/GetMessagesByRoomQueryHandler.cs`
- **Problema:** Qualquer usuário autenticado pode ler mensagens de qualquer sala sem ser membro. Não há verificação de pertinência à sala.
- **Impacto:** Violação grave de privacidade — vazamento de mensagens de salas privadas.
- **Fix:** Verificar `room.IsUserInRoom(user)` antes de retornar mensagens, igual ao padrão dos outros handlers.

---

#### C3. DeleteMessageCommandHandler — deleção do arquivo após o commit
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/DeleteMessage/DeleteMessageCommandHandler.cs:54-60`
- **Problema:** `UnitOfWork.Commit()` é chamado antes de deletar o arquivo no S3. Se a deleção do arquivo falhar, o registro da mensagem foi removido do banco mas o arquivo persiste — inconsistência permanente de dados.
- **Impacto:** Dados órfãos em S3; estado inconsistente irrecuperável.
- **Fix:** Deletar o arquivo S3 *antes* de chamar `Commit()`.

---

#### C4. Segredos hardcoded no repositório
- **Arquivos:** `src/ChatApp.Api/appsettings.json:9,12` · `src/ChatApp.Infrastructure/Database/EntityFramework/ChatAppDbContextFactory.cs:11`
- **Problema:** Chave JWT (`rV0xK+6G8xZJ3m9...`) e senha do banco (`postgres`) em texto puro commitados no repositório. Qualquer pessoa com acesso ao código pode comprometer o sistema.
- **Impacto:** Comprometimento total da autenticação e do banco em produção.
- **Fix:** Usar .NET User Secrets localmente; variáveis de ambiente em CI/CD e produção. Adicionar `appsettings.*.json` ao `.gitignore` se contiverem segredos.

---

#### C5. SignalR Hub sem autenticação e com parâmetro de usuário manipulável pelo cliente
- **Arquivo:** `src/ChatApp.Infrastructure/RealTime/ChatHub.cs:10-24`
- **Problema:** Métodos `JoinRoom`, `LeaveRoom` e `SendMessage` não têm `[Authorize]`. O objeto `User` é passado pelo *cliente* — qualquer um pode se passar por outro usuário.
- **Impacto:** Qualquer pessoa não autenticada pode enviar mensagens; ataque de impersonation é trivial.
- **Fix:** Adicionar `[Authorize]` no hub, extrair UserId do `Context.User` (JWT) e remover parâmetro `User` dos métodos públicos.

---

#### C6. Endpoint de criação de sala sem autorização
- **Arquivo:** `src/ChatApp.Api/Controllers/ChatRoomController.cs:22`
- **Problema:** `CreateChatRoom` não tem `[Authorize]`. Qualquer requisição não autenticada pode criar salas.
- **Fix:** Adicionar `[Authorize]` no método.

---

#### C7. Endpoint de Login inexistente
- **Arquivo:** `src/ChatApp.Api/Controllers/UserController.cs`
- **Problema:** Existe `LoginCommandHandler` e `LoginCommand`, mas o controller não expõe endpoint de login. Usuários registrados não têm como obter um token JWT.
- **Impacto:** A aplicação é inutilizável — nenhum fluxo de autenticação funciona end-to-end.
- **Fix:** Implementar `POST /api/user/login` que executa `LoginCommand` e retorna o token.

---

#### C8. Inconsistência no nome de grupo do SignalR (bug de conectividade)
- **Arquivo:** `src/ChatApp.Infrastructure/RealTime/ChatHub.cs:12` vs `src/ChatApp.Infrastructure/Services/SignalRChatRoomNotifier.cs:19,25,31`
- **Problema:** `ChatHub` usa o prefixo `"chat_{roomId}"` para o nome do grupo, mas `SignalRChatRoomNotifier` usa apenas `roomId` sem prefixo. Nenhuma notificação enviada pelo notifier chega aos clientes conectados.
- **Impacto:** Real-time completamente quebrado — mensagens não chegam aos usuários.
- **Fix:** Padronizar prefixo `"chat_"` no `SignalRChatRoomNotifier`.

---

### 🟠 ALTOS — Devem ser corrigidos antes de produção

---

#### A1. ContentType.From() não trata ContentType.Video
- **Arquivo:** `src/ChatApp.Domain/Entities/Messages/ContentType.cs:19-26`
- **Problema:** `Video` está declarado como campo estático mas não tem case no switch de `From()`. Lança `NotSupportedException` silenciosamente ao tentar enviar vídeo.
- **Fix:** Adicionar `"video" => Video` ao switch.

---

#### A2. ChatRoom.Join() silencia falhas — JoinRoomCommandHandler retorna sucesso indevidamente
- **Arquivos:** `src/ChatApp.Domain/Entities/ChatRooms/ChatRoom.cs:41-48` · `src/ChatApp.Application/UseCases/Rooms/JoinRoom/JoinRoomCommandHanlder.cs:52`
- **Problema:** `Join()` retorna `void` e silencia dois casos de falha (usuário já na sala, sala cheia). O handler chama `room.Join(user)` e retorna `Result.Success()` sem verificar se o join de fato ocorreu. O próprio teste documenta esse problema com um comentário.
- **Fix:** `Join()` deve retornar `Result`. O handler deve checar o resultado.

---

#### A3. EditMessageCommandHandler com tipo de retorno errado
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/EditMessage/EditMessageCommandHandler.cs:34`
- **Problema:** Handler implementa `ICommandHandler<EditMessageCommand>` (retorna `Result`), mas internamente chama `Result.Failure<Guid>()`. Inconsistência de tipo que pode gerar exceção de cast em runtime.
- **Fix:** Substituir todos os `Result.Failure<Guid>(...)` por `Result.Failure(...)`.

---

#### A4. Uso genérico de `Error.NullValue` para todo tipo de falha de negócio
- **Arquivos:** `ChatMessage.cs`, `LoginCommandHandler.cs`, `RegisterUserCommandHandler.cs`, `CreateRoomCommandHandler.cs`, `JoinRoomCommandHanlder.cs` e outros
- **Problema:** `Error.NullValue` é usado para "usuário não encontrado", "sala cheia", "mensagem muito antiga para editar", "credenciais inválidas" etc. O cliente não consegue distinguir o tipo de erro para dar feedback adequado. No login, usar o mesmo erro para "usuário não existe" e "senha errada" é insuficiente (ainda que correto para segurança), mas as mensagens deveriam ser tipadas corretamente.
- **Fix:** Criar classes de erros por domínio (`UserErrors`, `ChatRoomErrors`, `MessageErrors`) com códigos e mensagens descritivos. Exemplo: `MessageErrors.EditTimeExpired`, `ChatRoomErrors.MaxMembersReached`.

---

#### A5. User.cs sem validação de invariantes e modelo anêmico
- **Arquivo:** `src/ChatApp.Domain/Entities/Users/User.cs`
- **Problema:** Construtor não valida `name`, `username` ou `password`. Nenhuma regra de negócio reside no `User`. A entidade é apenas um container de dados.
- **Fix:** Adicionar validação no construtor (ou usar factory). Mover regras de negócio do usuário para a entidade.

---

#### A6. ChatRoom sem invariantes de criação
- **Arquivo:** `src/ChatApp.Domain/Entities/ChatRooms/ChatRoom.cs:19-29`
- **Problema:** Nome de sala pode ser nulo/vazio. Sala privada pode ser criada sem senha. `DateTime.UtcNow` é usado diretamente (dificulta testes).
- **Fix:** Validar nome e exigir senha para salas privadas. Receber `DateTime createdAt` como parâmetro no construtor.

---

#### A7. RegisterUserCommandHandler sem validação de entrada
- **Arquivo:** `src/ChatApp.Application/UseCases/Users/RegisterUser/RegisterUserCommandHandler.cs:26-46`
- **Problema:** Nome, username e senha não são validados (tamanho mínimo, formato etc.). Qualquer string chega ao banco.
- **Fix:** Adicionar validação de negócio no handler, ou implementar validators FluentValidation como pipeline behavior.

---

#### A8. UploadFileCommand usa `IRequest` diretamente em vez de `ICommand`
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/UploadFile/UploadFileCommand.cs:5-14`
- **Problema:** Quebra a consistência do padrão CQRS estabelecido no projeto. Não passa pelos pipeline behaviors (`LoggingBehavior`).
- **Fix:** Implementar `ICommand<UploadFileCommandResponse>`.

---

#### A9. UploadFileCommandHandler sem validação de arquivo
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/UploadFile/UploadFileCommandHandler.cs:16-25`
- **Problema:** Nenhuma validação de tamanho de arquivo, tipo de conteúdo permitido ou extensão. Um atacante pode enviar um arquivo de 10GB ou um `.exe` mascarado.
- **Fix:** Validar content-type contra whitelist, limitar tamanho de arquivo, validar extensão.

---

#### A10. `Entity.cs` — DomainEvents é uma lista pública mutável
- **Arquivo:** `src/ChatApp.Domain/Abstractions/Entity.cs`
- **Problema:** `public readonly List<IDomainEvent> DomainEvents` permite mutação externa da coleção. Viola encapsulamento e não é thread-safe.
- **Fix:** `private readonly List<IDomainEvent> _domainEvents; public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();`

---

#### A11. CreateRoomAsAnonymousCommandHandler não implementado
- **Arquivo:** `src/ChatApp.Application/UseCases/Rooms/CreateRoomAsAnonymous/CreateRoomAsAnonymousCommandHandler.cs:24-26`
- **Problema:** Lança `NotImplementedException`. Se registrado no MediatR, qualquer chamada explode em runtime.
- **Fix:** Implementar ou remover o feature até estar pronto.

---

#### A12. HTTPS desabilitado
- **Arquivo:** `src/ChatApp.Api/Program.cs:55`
- **Problema:** `app.UseHttpsRedirection()` está comentado. Tokens JWT transitam em plaintext sobre HTTP.
- **Fix:** Habilitar ao menos em produção: `if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();`

---

#### A13. JWT sem validação de Issuer/Audience
- **Arquivo:** `src/ChatApp.Infrastructure/DependencyInjection.cs:103-111`
- **Problema:** `ValidateIssuer = false` e `ValidateAudience = false` aceitam tokens de qualquer origem com a mesma chave.
- **Fix:** Configurar `ValidIssuer` e `ValidAudience` via `appsettings.json`.

---

#### A14. Sem constraint de unicidade no Username no banco
- **Arquivo:** `src/ChatApp.Infrastructure/Database/EntityFramework/Mappings/UserMapping.cs`
- **Problema:** Não há índice único em `Username`. Dois usuários podem ter o mesmo username, quebrando o login.
- **Fix:** Adicionar `builder.HasIndex(u => u.Username).IsUnique();` e criar migration.

---

#### A15. `ChatMessageRepository.Update` chama `SaveChangesAsync` diretamente
- **Arquivo:** `src/ChatApp.Infrastructure/Database/EntityFramework/Repositories/ChatMessageRepository.cs:32-36`
- **Problema:** Viola o padrão UnitOfWork. Commits parciais tornam transações inconsistentes.
- **Fix:** Remover `SaveChangesAsync()` — deixar o `UnitOfWork.Commit()` controlar o commit.

---

#### A16. Anti-pattern `async/await Task.CompletedTask` nos repositórios
- **Arquivo:** `src/ChatApp.Infrastructure/Database/EntityFramework/Repositories/ChatRoomRepository.cs:21-38`
- **Problema:** Métodos declarados `async` que executam `await Task.CompletedTask` — overhead desnecessário sem operação assíncrona real.
- **Fix:** Remover `async`, retornar `Task.CompletedTask` diretamente.

---

#### A17. Sem índices no banco para os campos de busca mais comuns
- **Arquivos:** `UserMapping.cs`, `ChatMessageMapping.cs`, `ChatRoomMapping.cs`
- **Problema:** `Username` (busca por login), `ChatMessage.ChatRoomId` (busca de mensagens da sala), `ChatMessage.SentAt` (ordenação e cursor de paginação) não têm índices. Full table scans desde o início.
- **Fix:** Adicionar índices via Fluent API e migration.

---

#### A18. Controllers com endpoints críticos faltando
- **Arquivo:** `src/ChatApp.Api/Controllers/`
- **Problema:** Os handlers de `JoinRoom`, `LeaveRoom`, `SendMessage`, `EditMessage`, `DeleteMessage` e `GetMessagesByRoom` existem na camada Application mas não têm endpoints HTTP correspondentes. A API é praticamente inutilizável como produto.
- **Fix:** Implementar os endpoints restantes com `[Authorize]`.

---

### 🟡 MÉDIOS — Melhorias importantes de qualidade e manutenibilidade

---

#### M1. `PerformanceBehavior` implementado mas não registrado no DI
- **Arquivo:** `src/ChatApp.Application/DependencyInjection.cs`
- **Fix:** `configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));`

---

#### M2. `GetMessagesByRoomQueryHandler` usa Dapper diretamente — impossível testar unitariamente
- **Arquivo:** `src/ChatApp.Application/UseCases/Messages/GetMessagesByRoom/GetMessagesByRoomQueryHandler.cs`
- **Problema:** Acessa banco via Dapper sem abstração. O teste correspondente já documenta isso com uma exceção intencional.
- **Fix:** Extrair para `IMessageQueryRepository` e injetar no handler.

---

#### M3. Rate limit muito restritivo para uma aplicação de chat
- **Arquivo:** `src/ChatApp.Infrastructure/DependencyInjection.cs:114-128`
- **Problema:** 10 requests/30s = 0.33 req/s. Um usuário digitando mensagens normalmente seria bloqueado.
- **Fix:** Criar políticas separadas: uma mais permissiva para chat (`100/min`) e uma mais restritiva para auth (`5/min`).

---

#### M4. Resposta de erro inconsistente entre controllers e ExceptionHandlingMiddleware
- **Arquivos:** `Controllers/*.cs` vs `ExceptionHandlingMiddleware.cs`
- **Problema:** Controllers retornam `{ error, message }` customizado; middleware retorna `ProblemDetails`. Clientes precisam tratar dois formatos.
- **Fix:** Padronizar tudo com `ProblemDetails` (RFC 7807).

---

#### M5. `ExceptionHandlingMiddleware` trata apenas `Exception` genérica
- **Arquivo:** `src/ChatApp.Api/Middlewares/ExceptionHandlingMiddleware.cs`
- **Fix:** Adicionar cases para `ValidationException` (400), `UnauthorizedAccessException` (403), `ArgumentException` (400).

---

#### M6. CORS com origens hardcoded no código
- **Arquivo:** `src/ChatApp.Api/Program.cs:22-30`
- **Fix:** Ler origens permitidas de `appsettings.json` via `configuration.GetSection("Cors:AllowedOrigins")`.

---

#### M7. Falta paginação nos repositórios
- **Arquivos:** Todos os repositórios
- **Problema:** Nenhum método de listagem com paginação. Carregar todas as mensagens/salas de uma vez é inviável em produção.
- **Fix:** Adicionar `GetPaged(int page, int size, CancellationToken)` nos repositórios.

---

#### M8. Inconsistência nas interfaces dos repositórios (async, cancellationToken)
- **Arquivo:** `IChatMessageRepository.cs` vs `IChatRoomRepository.cs`
- **Problema:** `Delete` em mensagens é `void` e recebe `CancellationToken` desnecessariamente. `IChatRoomRepository` usa `CancellationToken = default`, o outro não.
- **Fix:** Padronizar todos os métodos como `Task` com `CancellationToken cancellationToken = default`.

---

#### M9. Username com `MaxLength(200)` — excessivo e sem sentido
- **Arquivo:** `src/ChatApp.Infrastructure/Database/EntityFramework/Mappings/UserMapping.cs:21`
- **Fix:** `HasMaxLength(50)`.

---

#### M10. Password de sala de chat provavelmente armazenado em plaintext
- **Arquivo:** `src/ChatApp.Infrastructure/Database/EntityFramework/Mappings/ChatRoomMapping.cs:20-22` · `MaxLength(30)`
- **Problema:** Se a senha da sala é hasheada (bcrypt ~60 chars), o campo de 30 chars trunca o hash. Se é plaintext, é inseguro.
- **Fix:** Decidir estratégia: hashear senha de sala ou usar um modelo diferente de controle de acesso.

---

#### M11. `docker-compose.yml` sem healthcheck e depends_on
- **Arquivo:** `docker-compose.yml`
- **Problema:** API pode tentar conectar ao banco antes de ele estar pronto.
- **Fix:** Adicionar `healthcheck` no postgres e `depends_on: condition: service_healthy` na API. Trocar `postgres:latest` por versão fixada (`postgres:16-alpine`).

---

#### M12. `AsNoTracking()` ausente em queries de leitura
- **Arquivos:** Todos os repositórios em métodos `GetById`
- **Problema:** EF Core rastreia entidades desnecessariamente em operações de leitura.
- **Fix:** Adicionar `.AsNoTracking()` em todos os métodos que não retornam entidades para modificação.

---

#### M13. `ChatRoomRepository.GetById` não é `async` mas retorna `Task`
- **Arquivo:** `src/ChatApp.Infrastructure/Database/EntityFramework/Repositories/ChatRoomRepository.cs:27-31`
- **Problema:** Método não é `async` mas retorna `Task` — estilo inconsistente.
- **Fix:** Usar `async/await` padrão.

---

### 🔵 BAIXOS — Débito técnico e melhorias futuras

---

#### B1. Typos em nomes de arquivos e classes
- `JoinRoomCommandHanlder.cs` → `JoinRoomCommandHandler.cs`
- `RegisterUserCommnad.cs` → `RegisterUserCommand.cs`

---

#### B2. Domínio não usa Domain Events (infra preparada, mas não utilizada)
- `Entity.cs` tem suporte a domain events mas nenhuma entidade os emite. Eventos como `UserRegistered`, `MessageSent`, `RoomCreated` seriam a base para notificações, auditoria e integrações futuras.

---

#### B3. Magic numbers que deveriam ser configuráveis
- `MessageEditTimeLimitInHours = 1` e `MessageDeleteTimeLimitInHours = 24` em `ChatMessage.cs`
- Threshold de performance `500ms` em `PerformanceBehavior.cs`
- Tempo de expiração do presigned URL `10 minutos` em `UploadFileCommandHandler.cs`

---

#### B4. Nomenclatura inconsistente nos testes (PT-BR vs inglês)
- Mesmo dentro de um arquivo há mistura: `Handle_deve_criar_sala_publica` vs `Deveria_retornar_erro`.
- Sugestão: padronizar para inglês com padrão `Should_[action]_When_[condition]`.

---

#### B5. `await` desnecessário em verificações NSubstitute
- `await _unitOfWorkMock.Received(1).Commit(...)` — `Received()` não é awaitable; o `await` não faz nada aqui mas confunde.

---

#### B6. Sem testes para TODO implementado em ChatMessageTest.cs
- `test/ChatApp.UnitTests/Domain/Messages/ChatMessageTest.cs` tem método `Nao_deveria_permitir_deletar_mensagem_apos_limite_de_tempo` com apenas um comentário TODO.

---

#### B7. JWT expira em 1 hora sem refresh token
- `src/ChatApp.Infrastructure/Authentication/AuthenticationService.cs:33`
- Usuário precisa se logar novamente a cada hora. Considerar refresh token ou aumentar para 24h com rotação.

---

#### B8. Falta versionamento de API
- Sem `/api/v1/`, impossível evoluir a API sem quebrar clientes existentes.

---

## LISTA PRIORIZADA DE MELHORIAS

```
PRIORIDADE | ID  | PROBLEMA                                          | ESFORÇO
-----------+-----+---------------------------------------------------+---------
🔴 P1      | C1  | SendMessage retorna Guid errado                   | Mínimo
🔴 P1      | C7  | Endpoint de Login faltando                        | Baixo
🔴 P1      | C8  | SignalR group naming quebrado                     | Mínimo
🔴 P1      | C4  | Segredos hardcoded (JWT key, DB password)         | Baixo
🔴 P1      | C2  | GetMessages sem controle de acesso                | Baixo
🔴 P1      | C3  | Delete de arquivo após commit                     | Mínimo
🔴 P1      | C5  | SignalR Hub sem autenticação                      | Baixo
🔴 P1      | C6  | CreateChatRoom sem [Authorize]                    | Mínimo

🟠 P2      | A14 | Sem unique constraint no Username                 | Mínimo
🟠 P2      | A13 | JWT sem validar Issuer/Audience                   | Mínimo
🟠 P2      | A12 | HTTPS desabilitado                                | Mínimo
🟠 P2      | A18 | Endpoints HTTP faltando (Join, Leave, Send...)    | Alto
🟠 P2      | A4  | Error.NullValue genérico em todo o projeto        | Médio
🟠 P2      | A17 | Índices de banco faltando                         | Baixo
🟠 P2      | A2  | ChatRoom.Join() silencia falhas                   | Baixo
🟠 P2      | A3  | EditMessage tipo de retorno errado                | Mínimo
🟠 P2      | A15 | Repository chama SaveChanges diretamente          | Mínimo
🟠 P2      | A11 | CreateRoomAsAnonymous não implementado            | Médio
🟠 P2      | A7  | Sem validação em RegisterUser                     | Baixo
🟠 P2      | A5  | User.cs sem invariantes                           | Baixo
🟠 P2      | A6  | ChatRoom.cs sem invariantes de criação            | Baixo

🟡 P3      | M2  | GetMessages usa Dapper sem abstração              | Médio
🟡 P3      | M1  | PerformanceBehavior não registrado                | Mínimo
🟡 P3      | M4  | Resposta de erro inconsistente                    | Médio
🟡 P3      | M7  | Sem paginação nos repositórios                    | Médio
🟡 P3      | M3  | Rate limit inadequado para chat                   | Baixo
🟡 P3      | M12 | AsNoTracking ausente em queries de leitura        | Baixo
🟡 P3      | M6  | CORS hardcoded no código                          | Baixo
🟡 P3      | M8  | Interfaces de repositório inconsistentes          | Baixo
🟡 P3      | A8  | UploadFileCommand usa IRequest em vez de ICommand | Mínimo
🟡 P3      | A10 | DomainEvents lista pública mutável                | Mínimo
🟡 P3      | A9  | Sem validação de arquivo no upload                | Baixo
🟡 P3      | M10 | Password de sala possivelmente em plaintext       | Médio
🟡 P3      | M5  | ExceptionHandlingMiddleware muito genérico        | Baixo
🟡 P3      | M11 | Docker sem healthcheck/depends_on                 | Baixo

🔵 P4      | B1  | Typos em nomes de classes/arquivos                | Mínimo
🔵 P4      | B6  | Teste com TODO não implementado (ChatMessageTest) | Mínimo
🔵 P4      | A1  | ContentType.Video faltando no switch              | Mínimo
🔵 P4      | A16 | async/await Task.CompletedTask anti-pattern       | Baixo
🔵 P4      | B4  | Nomenclatura de testes inconsistente              | Baixo
🔵 P4      | B5  | await desnecessário em NSubstitute.Received()     | Mínimo
🔵 P4      | M9  | Username MaxLength(200) excessivo                 | Mínimo
🔵 P4      | M13 | GetById não-async retornando Task                 | Mínimo
🔵 P4      | B2  | Domain events preparados mas nunca emitidos       | Alto
🔵 P4      | B3  | Magic numbers sem configuração                    | Baixo
🔵 P4      | B7  | JWT expira em 1h sem refresh token                | Médio
🔵 P4      | B8  | Sem versionamento de API                          | Médio
```

---

## RESUMO EXECUTIVO

**O projeto tem uma arquitetura limpa e bem estruturada.** O padrão Clean Architecture, CQRS via MediatR, Result pattern e separação de responsabilidades estão bem aplicados.

**Porém, o projeto tem problemas que o impedem de ir para produção:**
- A aplicação não tem um fluxo de login funcional (endpoint inexistente)
- O real-time está quebrado (group naming inconsistente)
- Segredos estão expostos no repositório
- Não há autorização nos endpoints e hub mais críticos
- Um bug devolve IDs errados ao criar mensagens

**Em números:** 8 críticos · 18 altos · 13 médios · 9 baixos = **48 issues no total**

**Caminho recomendado:**
1. **Sprint 1 (P1):** Corrigir os 8 críticos — levar menos de 2 dias
2. **Sprint 2 (P2):** Implementar endpoints faltando + correções altas — ~1 semana
3. **Sprint 3 (P3):** Qualidade e manutenibilidade — ~1 semana
4. **Backlog (P4):** Melhorias incrementais conforme tempo permite

---

## VERIFICAÇÃO

Após as correções, validar:
1. `dotnet build` sem warnings
2. `dotnet test` — todos os testes passando (incluindo implementar o teste TODO do `ChatMessageTest.cs`)
3. Fluxo completo manual: Register → Login → Create Room → Join Room → Send Message (via SignalR e REST) → Edit → Delete
4. Verificar que usuário de outra sala não consegue ler mensagens de salas que não pertence
5. Confirmar que segredos não estão em arquivos commitados (`git grep "postgres" -- "*.json"`)
