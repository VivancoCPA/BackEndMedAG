using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.CenterTypes.GetCenterType;

// ── Response ────────────────────────────────────────────────────
public record GetCenterTypeResponse(int Id, string Name, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class GetCenterTypeQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public GetCenterTypeQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<GetCenterTypeResponse?> HandleAsync(int id, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id         AS Id,
                   name       AS Name,
                   is_active  AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM centers_type
            WHERE id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<GetCenterTypeResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
