# Security baseline

## State of v0.1

- Ingestion and incident retrieval require an exact bearer token.
- The Meta webhook verifies X-Hub-Signature-256 over **raw body bytes**.
- Meta subscription challenge uses a dedicated verification token.
- Missing WhatsApp secrets cause fail-closed responses.
- Validated webhooks return 202 and are discarded; **no commands run**.
- Docker Compose binds the API only to localhost by default.
- PostgreSQL is reachable from the Docker network, not published publicly.
- CI has contents:read; no deployment or write token.
- .env, certificates and local secrets are gitignored.

## Before public ingress

Implement an authenticated reverse proxy (TLS + rate limits + size limits), replay protection, bounded body reads, request tracing, secure secret distribution and persistence of sanitized events. This sample API is for local development only. Never expose its single static bearer token endpoint directly to the public internet.

## Before PR merge and deploy

- Dedicated least-privilege GitHub App; protected branches and required checks.
- PR approval must be bound to the **exact head SHA**, checks and repository.
- Strong identity for human approvers, challenge expiry, nonce, anti-replay.
- Separate permissions for approval, merge, staging and production release.
- Ephemeral execution sandboxes without production credentials.
- Egress allowlists, tool ACLs, protected file paths, resource quotas.
- Treat issue text, logs, source and WhatsApp content as untrusted input.
- Audit trail of every decision; avoid storing tokens or PII in prompts.
- Canary and health checks before considering rollback automation.
- Require explicit consent and a documented procedure before touching production.

## Reporting a vulnerability

Do not post exploitable details in a public issue. Contact the maintainer privately through GitHub.
