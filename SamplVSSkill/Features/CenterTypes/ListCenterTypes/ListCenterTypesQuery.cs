using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.CenterTypes.ListCenterTypes;

// ── Response ────────────────────────────────────────────────────
public record ListCenterTypesResponse(int Id, string Name, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListCenterTypesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListCenterTypesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListCenterTypesResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id         AS Id,
                   name       AS Name,
                   is_active  AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM centers_type
            ORDER BY created_at
            """;

        return await connection.QueryAsync<ListCenterTypesResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
