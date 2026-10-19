# Claudear integration (planned)

Upstream: https://github.com/appwrite/claudear

The initial approach is an adapter around the upstream service/CLI and webhook lifecycle, rather than copying or vendoring its Rust implementation. Verify upstream APIs, CLI contracts, license, notices, security constraints and release stability before implementation.

Proposed interface:
- Subscribe to observed incidents/issue candidates.
- Request a bounded issue-to-PR task.
- Receive PR SHA, link, checks and evidence.
- Propagate state changes into Sentinel audit workflow.

No upstream source code is bundled and no Claudear executable is launched by v0.1.
