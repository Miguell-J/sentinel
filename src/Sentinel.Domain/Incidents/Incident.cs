namespace Sentinel.Domain.Incidents;

public sealed record NewIncident(string Source, string Repository, string Fingerprint, string Title);

public sealed record Incident(
    Guid Id, string Source, string Repository, string Fingerprint, string Title,
    IncidentStatus Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
