using Npgsql;
using Sentinel.Domain.Incidents;

namespace Sentinel.Infrastructure.Persistence;

public sealed class PostgresIncidentStore(NpgsqlDataSource dataSource) : IIncidentStore
{
    private const string Columns = "id, source, repository, fingerprint, title, status, created_at, updated_at";

    public async Task<Incident> AddOrGetAsync(NewIncident request, CancellationToken cancellationToken)
    {
        var sql = $"INSERT INTO incidents (id, source, repository, fingerprint, title, status) " +
                  $"VALUES (@id, @source, @repository, @fingerprint, @title, 'Detected') " +
                  $"ON CONFLICT (source, repository, fingerprint) DO UPDATE " +
                  $"SET title = EXCLUDED.title, updated_at = now() RETURNING {Columns};";
        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.AddWithValue("source", request.Source);
        command.Parameters.AddWithValue("repository", request.Repository);
        command.Parameters.AddWithValue("fingerprint", request.Fingerprint);
        command.Parameters.AddWithValue("title", request.Title);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Insert returned no incident.");
        return ReadIncident(reader);
    }

    public async Task<Incident?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"SELECT {Columns} FROM incidents WHERE id = @id;");
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadIncident(reader) : null;
    }

    public async Task<long> CountOpenAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT COUNT(*) FROM incidents WHERE status NOT IN ('Resolved', 'Failed', 'RolledBack', 'Rejected');");
        return (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var command = dataSource.CreateCommand("SELECT 1;");
            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 1;
        }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException or InvalidOperationException)
        {
            return false;
        }
    }

    private static Incident ReadIncident(NpgsqlDataReader reader) => new(
        reader.GetGuid(0), reader.GetString(1), reader.GetString(2),
        reader.GetString(3), reader.GetString(4),
        Enum.Parse<IncidentStatus>(reader.GetString(5)),
        reader.GetFieldValue<DateTimeOffset>(6), reader.GetFieldValue<DateTimeOffset>(7));
}
