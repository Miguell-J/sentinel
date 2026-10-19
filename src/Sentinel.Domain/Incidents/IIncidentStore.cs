namespace Sentinel.Domain.Incidents;

public interface IIncidentStore
{
    Task<Incident> AddOrGetAsync(NewIncident request, CancellationToken cancellationToken);
    Task<Incident?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<long> CountOpenAsync(CancellationToken cancellationToken);
    Task<bool> PingAsync(CancellationToken cancellationToken);
}
