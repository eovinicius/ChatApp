# ChatApp

ChatApp é uma aplicação de chat desenvolvida em .NET, estruturada em múltiplos projetos para separar responsabilidades de domínio, aplicação, infraestrutura, API e testes.

## Visão Geral

O ChatApp permite a comunicação em tempo real entre usuários, com funcionalidades de cadastro, autenticação, criação de salas e histórico de mensagens. O projeto segue boas práticas de arquitetura, separando as camadas de domínio, aplicação, infraestrutura, API e testes.

## Arquitetura

- **Domain:** Entidades, agregados e regras de negócio.
- **Application:** Casos de uso, DTOs e lógica de orquestração.
- **Infrastructure:** Persistência de dados (Entity Framework Core), integrações externas.
- **API:** Endpoints REST para interação com clientes.
- **Testes:** Testes unitários com xUnit.

Estrutura de pastas:

```
src/
  ChatApp.Domain/
  ChatApp.Application/
  ChatApp.Infrastructure/
  ChatApp.Api/
Test/
  ChatApp.UnitTests/
```

## Tecnologias Utilizadas

- .NET 8+ (C#)
- ASP.NET Core (Web API)
- Entity Framework Core (ORM)
- MediatR (para mediadores de comandos e consultas)
- SignalR (para comunicação em tempo real)
- xUnit (testes unitários)
- Swagger (documentação da API)
- Docker (para containerização)
- PostgreSQL (banco de dados)
- Redis (para cache e mensagens em tempo real)
- FluentValidation (validação de modelos)
- Serilog (logging)

## Como Executar

1. Clone o repositório:
   ```sh
   git clone https://github.com/eovinicius/ChatApp.git
   ```
2. Restaure os pacotes NuGet:
   ```sh
   dotnet restore
   ```
3. Compile a solução:
   ```sh
   dotnet build
   ```
4. Execute a API:
   ```sh
   cd src/ChatApp.Api
   dotnet run
   ```
5. Acesse a API em `https://localhost:5001` ou `http://localhost:5000`.

## Executando os Testes

```sh
dotnet test
```

## Funcionalidades

- Cadastro e autenticação de usuários
- Criação de salas de chat
- Envio e recebimento de mensagens em tempo real
- Histórico de conversas

## Exemplos de Endpoints

### Cadastro de Usuário

```http
POST /api/users/register
Content-Type: application/json

{
  "username": "usuario",
  "password": "senha"
}
```

### Login

```http
POST /api/users/login
Content-Type: application/json

{
  "username": "usuario",
  "password": "senha"
}
```

### Criar Sala

```http
POST /api/rooms
Content-Type: application/json

{
  "name": "Sala Geral"
}
```

### Enviar Mensagem

```http
POST /api/messages
Content-Type: application/json

{
  "roomId": "id-da-sala",
  "content": "Olá, mundo!"
}
```

### Listar Mensagens de uma Sala

```http
GET /api/rooms/{roomId}/messages
```

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch (`git checkout -b feature/NovaFuncionalidade`)
3. Commit suas alterações (`git commit -am 'Adiciona nova funcionalidade'`)
4. Faça push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

## Licença

Este projeto está licenciado sob a licença MIT.

## Contato

Dúvidas ou sugestões? Entre em contato pelo [eovinicius10@gmail.com](mailto:seu-email@dominio.com).
