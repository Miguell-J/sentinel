# WhatsApp approval gateway (design only)

The Meta Cloud API sends verification requests and signed events to the configured webhook.

v0.1 supports:
- GET /webhooks/whatsapp — challenge verification using WhatsApp:VerifyToken
- POST /webhooks/whatsapp — validate HMAC-SHA256 using WhatsApp:AppSecret; return 202 **without processing the payload**

v0.1 does **not** send messages or approve anything.

Future request record:

    approvalId, tenantId, recipientPhoneHash, approverIdentity,
    repository, prNumber, immutableCommitSha, checksManifestSha,
    riskLevel, expiresAt, nonce, status

Workflow:
1. Confirm agent output and CI checks with GitHub API.
2. Authorize human approver for specific repository/environment.
3. Send an interactive message containing minimal incident summary.
4. Verify Meta webhook signature and deduplicate event ID.
5. Resolve reply against unique, unexpired request (never plain "yes").
6. Recheck head SHA, PR status, checks, and branch policy at merge time.
7. Dispatch a short-lived scoped authorization to Delivery Controller.
8. Persist decision and send confirmation/rejection.

WhatsApp approval alone must not grant production deployment.
