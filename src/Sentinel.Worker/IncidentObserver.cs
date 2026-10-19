using Sentinel.Domain.Incidents;

namespace Sentinel.Worker;

// Read-only v0.1 worker: no agent execution, merge or deployment.
public sealed class IncidentObserver(IIncidentStore incidents, ILogger<IncidentObserver> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var count = await incidents.CountOpenAsync(stoppingToken);
                logger.LogInformation("Sentinel observer: {Count} open incidents; execution disabled", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Sentinel observer cannot query incidents.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }
}
