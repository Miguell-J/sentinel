using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sentinel.Domain.Incidents;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSentinelInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Sentinel");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:Sentinel must be configured.");

        services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        services.AddSingleton<IIncidentStore, PostgresIncidentStore>();
        return services;
    }
}
