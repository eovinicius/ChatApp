# CI/CD — Estrutura de Workflows (Monólito Modular)

Este repositório é um **monólito modular** sob `app/` (módulos `Chat`, `Identity`, `Notification` compostos por `ChatApp.Api`). A estrutura de workflows usa jobs reutilizáveis, prontos para crescer sem duplicar lógica.

## Estrutura

```
.github/workflows/
├── ci.yml                   # CI: build + test da solution (app/**, tests/**, ChatApp.slnx)
├── cd.yml                    # CD: build da imagem única (ChatApp.Api), push ECR, deploy ECS (jobs dev/prod)
├── codeql.yml                 # Análise de segurança CodeQL
├── terraform-plan.yml        # Terraform plan (PR) — um job por ambiente (dev/staging/prod)
├── terraform-apply.yml       # Terraform apply (push em main) — um job por ambiente
├── reusable/
│   ├── dotnet-ci.yml          # restore + build + test genérico (workflow_call)
│   ├── docker-cd.yml         # build/push ECR + deploy ECS genérico (workflow_call)
│   └── terraform.yml         # plan/apply genérico por ambiente (workflow_call)
└── README.md
```

## Reusable workflows

| Arquivo | Inputs principais | Secrets | Usado por |
|---|---|---|---|
| `reusable/dotnet-ci.yml` | `working-directory`, `solution-file`, `dotnet-version` | — | `ci.yml` |
| `reusable/docker-cd.yml` | `dockerfile`, `context`, `ecr-repository`, `ecs-cluster`, `ecs-service`, `environment-name`, `sha` | `aws-role-arn`, `aws-region` | `cd.yml` |
| `reusable/terraform.yml` | `environment` (nome do GitHub Environment), `label` (nome curto: dev/staging/prod), `working-directory`, `action` (plan\|apply) | `aws-role-arn`, `aws-region`, `jwt-secret-key` | `terraform-plan.yml`, `terraform-apply.yml` |

## ⚠️ Armadilha conhecida: `cd.yml` depende do `name:` de `ci.yml`

`cd.yml` dispara via `workflow_run: workflows: ["CI"]`. Essa string precisa bater **exatamente** com o campo `name:` no topo de `ci.yml`. Se um dos dois for renomeado sem atualizar o outro, o CD simplesmente **nunca dispara** — sem nenhum erro visível no GitHub Actions.

## Terraform: ambientes

`infra/environments/{dev,staging,prod}` já existem (cada um com seu próprio state no S3). Cada ambiente tem seu **próprio job** em `terraform-plan.yml` / `terraform-apply.yml`, e um `if:` controla quando ele roda:

- **`prod`** roda automaticamente (`plan` em PRs que tocam `infra/**`; `apply` em push para `main`) e também via `workflow_dispatch`.
- **`dev`/`staging`** só rodam via `workflow_dispatch` (botão "Run workflow" no GitHub, escolhendo o ambiente).

> Nota: o contexto `matrix` **não** pode ser usado no `if:` de nível de job (o GitHub avalia o `if` antes de expandir a matriz). Por isso cada ambiente é um job separado com `if` literal, em vez de uma matrix com flag `auto`.

Para promover `dev` ou `staging` a automático: no job correspondente (`plan-dev`/`apply-dev`, etc.), adicione ao `if:` a mesma condição de push/PR do job `prod`. Pré-requisito: o secret de role ARN do ambiente precisa existir (`AWS_ROLE_ARN_DEV` já existe; `AWS_ROLE_ARN_STAGING` ainda não foi criado).

Em cada job, `environment` é o nome do GitHub Environment (protection rules — `production`/`development`/`staging`), separado de `label` (nome curto usado para montar o caminho `infra/environments/<label>`), para preservar as protection rules já configuradas em `production`.

## Adicionando um novo módulo

Por ser um **monólito modular**, há um **único deployable** (`ChatApp.Api`): todos os módulos sobem juntos, na mesma imagem e no mesmo serviço ECS. Adicionar um módulo **não requer novos workflows**:

1. Criar `app/Modules/<Novo>` (com `src/` e `tests/`) e adicioná-lo à solution `ChatApp.slnx`.
2. Referenciar `<Novo>.Presentation` no `ChatApp.Api` e compor no `Program.cs` (`Add<Novo>Module` / `Map<Novo>Endpoints`).
3. Pronto — `ci.yml` já compila/testa a solution inteira e `cd.yml` já publica a imagem única com o novo módulo incluído.

Um workflow novo de CI/CD só faz sentido se surgir um **segundo deployable separado** (ex.: um worker ou gateway em processo próprio). Nesse caso, copie `ci.yml`/`cd.yml` para o novo alvo e ajuste `paths`, `name`, `workflow_run.workflows` e os valores de deploy.

## Fora de escopo (deliberado)

- **Pin de actions por SHA**: hoje usam tags (`@v4`, `@v6`...). Mais seguro contra supply-chain, mas fica como recomendação futura — trocar sem poder validar localmente tem risco de quebrar por SHA errado.
- **`dependabot-auto-merge.yml`**: auto-merge de PRs do Dependabot é uma automação nova (não uma reorganização do que já existe) e tem implicação de segurança (merge sem revisão humana). Fica como sugestão de nome para quando for implementado deliberadamente.
