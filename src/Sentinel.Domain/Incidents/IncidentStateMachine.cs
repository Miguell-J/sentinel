namespace Sentinel.Domain.Incidents;

// Domain model only. No externally callable transition endpoints in v0.1.
public static class IncidentStateMachine
{
    private static readonly IReadOnlyDictionary<IncidentStatus, IncidentStatus[]> Allowed =
        new Dictionary<IncidentStatus, IncidentStatus[]>
        {
            [IncidentStatus.Detected] = [IncidentStatus.Triaged, IncidentStatus.Failed],
            [IncidentStatus.Triaged] = [IncidentStatus.Investigating, IncidentStatus.Failed],
            [IncidentStatus.Investigating] = [IncidentStatus.FixProposed, IncidentStatus.Failed],
            [IncidentStatus.FixProposed] = [IncidentStatus.AwaitingApproval, IncidentStatus.Failed],
            [IncidentStatus.AwaitingApproval] = [IncidentStatus.Approved, IncidentStatus.Rejected],
            [IncidentStatus.Approved] = [IncidentStatus.Deploying, IncidentStatus.Failed],
            [IncidentStatus.Deploying] = [IncidentStatus.Verifying, IncidentStatus.Failed, IncidentStatus.RolledBack],
            [IncidentStatus.Verifying] = [IncidentStatus.Resolved, IncidentStatus.Failed, IncidentStatus.RolledBack]
        };

    public static bool CanTransition(IncidentStatus from, IncidentStatus to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
