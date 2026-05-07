using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Specialties.GetSpecialty;

// ── Response ────────────────────────────────────────────────────
public record GetSpecialtyResponse(int Id, string Name, bool IsActive, DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class GetSpecialtyQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public GetSpecialtyQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<GetSpecialtyResponse?> HandleAsync(int id, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id         AS Id,
                   name       AS Name,
                   is_active  AS IsActive,
                   created_at AS CreatedAt
            FROM specialties
            WHERE id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<GetSpecialtyResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
