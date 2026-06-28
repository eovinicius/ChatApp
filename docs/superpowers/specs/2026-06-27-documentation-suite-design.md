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
