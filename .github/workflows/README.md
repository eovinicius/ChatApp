# CI/CD — Estrutura de Workflows (Monorepo)

Este repositório é um **monólito modular** sob `src/` (módulos `Chat`, `Identity`, `Notification` compostos por `ChatApp.Api`). A estrutura de workflows usa jobs reutilizáveis, prontos para crescer sem duplicar lógica.

## Estrutura

```
.github/workflows/
├── chat-ci.yml              # CI: build + test da solution, dispara em src/** e tests/**
├── chat-cd.yml               # CD do Chat: build de imagem, push ECR, deploy ECS (dev/prod)
├── codeql.yml                 # Análise de segurança CodeQL — matrix por serviço
├── terraform-plan.yml        # Terraform plan (PR) — matrix por ambiente (dev/staging/prod)
├── terraform-apply.yml       # Terraform apply (push em main) — matrix por ambiente
├── reusable/
│   ├── dotnet-ci.yml          # restore + build + test genérico (workflow_call)
│   ├── docker-cd.yml         # build/push ECR + deploy ECS genérico (workflow_call)
│   └── terraform.yml         # plan/apply genérico por ambiente (workflow_call)
└── README.md
```

## Reusable workflows

| Arquivo | Inputs principais | Secrets | Usado por |
|---|---|---|---|
| `reusable/dotnet-ci.yml` | `working-directory`, `solution-file`, `dotnet-version` | — | `chat-ci.yml` |
| `reusable/docker-cd.yml` | `dockerfile`, `context`, `ecr-repository`, `ecs-cluster`, `ecs-service`, `environment-name`, `sha` | `aws-role-arn`, `aws-region` | `chat-cd.yml` |
| `reusable/terraform.yml` | `environment` (nome do GitHub Environment), `label` (nome curto: dev/staging/prod), `working-directory`, `action` (plan\|apply) | `aws-role-arn`, `aws-region`, `jwt-secret-key` | `terraform-plan.yml`, `terraform-apply.yml` |

## ⚠️ Armadilha conhecida: `chat-cd.yml` depende do `name:` de `chat-ci.yml`

`chat-cd.yml` dispara via `workflow_run: workflows: ["Chat CI"]`. Essa string precisa bater **exatamente** com o campo `name:` no topo de `chat-ci.yml`. Se um dos dois for renomeado sem atualizar o outro, o CD simplesmente **nunca dispara** — sem nenhum erro visível no GitHub Actions. O mesmo vale para qualquer par `<serviço>-ci.yml` / `<serviço>-cd.yml` futuro.

## Terraform: ambientes

`infra/environments/{dev,staging,prod}` já existem (cada um com seu próprio state no S3). Os workflows de Terraform usam uma matrix com um flag `auto` por ambiente:

- **`auto: true`** roda automaticamente (`terraform-plan.yml` em PRs que tocam `infra/**`; `terraform-apply.yml` em push para `main`). Hoje só `prod` tem `auto: true`, preservando o comportamento anterior.
- **`auto: false`** só roda via `workflow_dispatch` (botão "Run workflow" no GitHub, escolhendo o ambiente). É o caso de `dev`/`staging` hoje.

Para promover `dev` ou `staging` a automático: troque `auto: false` → `auto: true` na entrada correspondente do `matrix.include` — **não precisa reescrever o workflow**. Pré-requisito: o secret de role ARN do ambiente precisa existir (`AWS_ROLE_ARN_DEV` já existe; `AWS_ROLE_ARN_STAGING` ainda não foi criado).

Cada entrada da matrix tem `environment` (nome do GitHub Environment, usado para protection rules — `production`/`development`/`staging`) separado de `label` (nome curto usado só para exibição e para montar o caminho `infra/environments/<label>`), para preservar as protection rules já configuradas em `production`.

## Como adicionar um novo serviço (ex: `notification-service`)

1. Criar `notification-ci.yml` copiando `chat-ci.yml`, trocando `name:`, `paths:` (`apps/notification-service/**`) e os `with:` do `working-directory`/`solution-file`.
2. Criar `notification-cd.yml` copiando `chat-cd.yml`, trocando o `workflow_run.workflows` (para bater com o `name:` do passo 1) e a matrix (`ecr-repository`/`ecs-cluster`/`ecs-service`/secrets do novo serviço).
3. Adicionar uma entrada em `codeql.yml`'s `strategy.matrix.include` apontando para o novo serviço.
4. Nenhum arquivo do Chat precisa ser tocado.

## Fora de escopo (deliberado)

- **Pin de actions por SHA**: hoje usam tags (`@v4`, `@v6`...). Mais seguro contra supply-chain, mas fica como recomendação futura — trocar sem poder validar localmente tem risco de quebrar por SHA errado.
- **`dependabot-auto-merge.yml`**: auto-merge de PRs do Dependabot é uma automação nova (não uma reorganização do que já existe) e tem implicação de segurança (merge sem revisão humana). Fica como sugestão de nome para quando for implementado deliberadamente.
