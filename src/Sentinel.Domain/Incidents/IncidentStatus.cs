namespace Sentinel.Domain.Incidents;

public enum IncidentStatus
{
    Detected, Triaged, Investigating, FixProposed, AwaitingApproval,
    Approved, Rejected, Deploying, Verifying, Resolved, Failed, RolledBack
}
