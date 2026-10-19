using Sentinel.Domain.Incidents;

// Minimal dependency-free smoke tests. A richer test runner will follow.
var tests = new (IncidentStatus From, IncidentStatus To, bool Allowed)[]
{
    (IncidentStatus.Detected, IncidentStatus.Triaged, true),
    (IncidentStatus.Detected, IncidentStatus.Deploying, false),
    (IncidentStatus.Triaged, IncidentStatus.Investigating, true),
    (IncidentStatus.Investigating, IncidentStatus.FixProposed, true),
    (IncidentStatus.FixProposed, IncidentStatus.AwaitingApproval, true),
    (IncidentStatus.AwaitingApproval, IncidentStatus.Approved, true),
    (IncidentStatus.AwaitingApproval, IncidentStatus.Deploying, false),
    (IncidentStatus.Approved, IncidentStatus.Deploying, true),
    (IncidentStatus.Deploying, IncidentStatus.Verifying, true),
    (IncidentStatus.Verifying, IncidentStatus.Resolved, true),
    (IncidentStatus.Rejected, IncidentStatus.Deploying, false),
    (IncidentStatus.Resolved, IncidentStatus.Triaged, false),
    (IncidentStatus.Failed, IncidentStatus.Approved, false)
};

foreach (var (from, to, allowed) in tests)
{
    if (IncidentStateMachine.CanTransition(from, to) != allowed)
        throw new InvalidOperationException($"State transition mismatch: {from} -> {to}.");
}

Console.WriteLine($"Sentinel domain checks passed: {tests.Length} transitions verified.");
