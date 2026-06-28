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
