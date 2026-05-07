using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Specialties.ListSpecialties;

// ── Response ────────────────────────────────────────────────────
public record ListSpecialtiesResponse(int Id, string Name, bool IsActive, DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListSpecialtiesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListSpecialtiesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListSpecialtiesResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id         AS Id,
                   name       AS Name,
                   is_active  AS IsActive,
                   created_at AS CreatedAt
            FROM specialties
            ORDER BY name
            """;

        return await connection.QueryAsync<ListSpecialtiesResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
