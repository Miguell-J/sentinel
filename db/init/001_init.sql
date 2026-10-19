-- Executed only on creation of a new PostgreSQL volume. Use migrations for upgrades.
CREATE TABLE IF NOT EXISTS incidents (
    id uuid PRIMARY KEY,
    source text NOT NULL,
    repository text NOT NULL,
    fingerprint text NOT NULL,
    title text NOT NULL,
    status text NOT NULL DEFAULT 'Detected' CHECK (status IN (
        'Detected', 'Triaged', 'Investigating', 'FixProposed',
        'AwaitingApproval', 'Approved', 'Rejected', 'Deploying',
        'Verifying', 'Resolved', 'Failed', 'RolledBack'
    )),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT incidents_dedup UNIQUE (source, repository, fingerprint)
);
CREATE INDEX IF NOT EXISTS ix_incidents_status ON incidents (status, created_at);

CREATE TABLE IF NOT EXISTS incident_events (
    id uuid PRIMARY KEY,
    incident_id uuid NOT NULL REFERENCES incidents(id),
    event_type text NOT NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    occurred_at timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_incident_events_incident ON incident_events (incident_id, occurred_at);

-- Reserved for human-approved workflows. No write endpoints are enabled.
CREATE TABLE IF NOT EXISTS approvals (
    id uuid PRIMARY KEY,
    incident_id uuid NOT NULL REFERENCES incidents(id),
    repository text NOT NULL,
    pull_request_number integer NOT NULL,
    commit_sha text NOT NULL CHECK (length(commit_sha) IN (40, 64)),
    approver_identity text,
    status text NOT NULL DEFAULT 'Pending'
        CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Expired', 'Revoked')),
    expires_at timestamptz NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    decided_at timestamptz
);
CREATE INDEX IF NOT EXISTS ix_approvals_pending ON approvals (status, expires_at);
