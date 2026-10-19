# Sentinel

**Sentinel** is a human-supervised autonomous software engineering platform. It aims to connect incident detection, AI agents (Claude Code and Codex), GitHub pull requests, WhatsApp approvals, and controlled deployment.

> **Status: bootstrap / v0.1 foundation.** This repository does **not** run AI agents, approve PRs, merge code, or deploy automatically yet. The WhatsApp webhook only verifies incoming requests. No production credentials should be supplied to agents.

## Included in the bootstrap

- .NET 10 API and read-only background worker
- PostgreSQL incident storage and initial schema
- Versioned incident state model and domain tests
- Authenticated incident ingestion and query API
- Signature-validated (non-executing) Meta WhatsApp webhook scaffold
- Docker Compose for local development, CI, security policy, architecture RFC
- Adapter boundaries for Claudear, Cordis, MCP One / MCP Stack

## Quick start

```bash
cp .env.example .env
# Edit .env; provide unique, strong values for POSTGRES_PASSWORD and SENTINEL_INGEST_TOKEN
docker compose up --build -d
curl http://localhost:8080/health/live
```

To create an incident:

```bash
curl -X POST http://localhost:8080/api/v1/incidents \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer YOUR_SENTINEL_INGEST_TOKEN" \
  -d '{"source":"manual","repository":"example/api","fingerprint":"sample-error-1","title":"Example incident"}'
```

Read `docs/architecture.md`, `docs/security.md`, and `docs/RFC-001.md` before integrating real systems.

## Architecture

```text
sources (Sentry, GitHub, webhooks)
               |
         Sentinel API ---- PostgreSQL
               |
      Control / Policy Plane
               |
    Worker -> Agent Runtime (future)
               |
    PR -> Approval Gateway (future)
               |
    CI -> Release Controller (future)
```

The Claudear integration is **planned**, not vendored; see `integrations/claudear/README.md`. WhatsApp approval and deployment are gated future milestones.

## Scope and safeguards

- Human approval must be bound to an immutable commit SHA.
- Branch protection and CI are required before any merge.
- Agents must run in ephemeral, least-privilege environments, never with production secrets.
- Inbound WhatsApp messages are **not** commands, and cannot trigger deployment.
- Use an isolated test repository to validate the incident-to-PR flow before production.

## License

No license has been selected for Sentinel yet. Do not copy Claudear or Cordis code into this repository until their notices, licenses, and integration contracts have been reviewed.
