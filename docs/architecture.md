# Sentinel — Arquitetura inicial

## Missão

Plataforma de engenharia de software autônoma **supervisionada**: transformar incidentes em tarefas reproduzíveis, PRs revisáveis e entregas controladas.

## Estado da implementação

| Camada | v0.1 | Próxima etapa |
|---|---|---|
| Domain | Incidentes e máquina de estados | Eventos de domínio versionados |
| API | Ingestão autenticada + consulta + health checks | AuthN/AuthZ multiusuário |
| Persistence | PostgreSQL + tabelas iniciais | Migrations, outbox e audit trail |
| Worker | Consulta read-only de incidentes | Durable workflows com retries |
| Meta WhatsApp | Validação de assinatura + challenge | Identidades, aprovações e templates |
| Agent Runtime | Contrato arquitetural | Adapters Claude/Codex isolados |
| Claudear | Planejado | Adapter + license review |
| Delivery | Não implementado | GitHub App + CI/merge/deploy com gates |

### Contexto

    [Sentry / GitHub / manuais]
               |
          [Sentinel API] ------------------ [PostgreSQL]
               |
         [Incident Domain]
               |
          [Control Plane]  (futuro)
               |
        [Workflow Worker]
           /          \
      [Claude]       [Codex]  (futuros; containers sem segredos)
           \          /
           [GitHub PR]        (futuro)
                |
       [WhatsApp Approval]   (futuro; SHA assinado + identidade)
                |
      [Delivery Controller]  (futuro; estágio -> produção)

### Limites

- API: HTTP, autenticação de ingestão e validação de webhooks; não executa LLMs.
- Domain: sem dependências de ASP.NET, bancos ou fornecedores.
- Infrastructure: persistência parametrizada e contratos do domínio.
- Worker: processamento assíncrono; v0.1 somente consulta e telemetria.
- Agent Runtime: futuro plugin de executores; ferramentas governadas por políticas.
- Delivery: futuro serviço de alto privilégio separado do runtime do agente.

## Regras

1. Nenhum evento externo pode conceder autoridade diretamente a um executor.
2. Todo evento externo deve possuir deduplicação e idempotência.
3. Aprovações vinculadas a repository + PR + commit SHA + expiração + identidade.
4. Nunca reutilizar tokens do GitHub/WhatsApp dentro de workspaces dos agentes.
5. Separar merge, deploy e rollback como operações independentes e auditáveis.
6. Workflows retomáveis e idempotentes; nenhum processo espera indefinidamente por humanos.
7. Mudanças em schemas de produção exigem migrações versionadas e rollback analisado.

Leia RFC-001 e security.md antes de adicionar integrações.
