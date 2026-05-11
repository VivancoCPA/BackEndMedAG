using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Specialties.LookupSpecialties;

// ── Query Handler (Dapper) ──────────────────────────────────────
public class LookupSpecialtiesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public LookupSpecialtiesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<LookupItem>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id   AS Id,
                   name AS Name
            FROM specialties
            WHERE is_active = true
            ORDER BY name
            """;

        return await connection.QueryAsync<LookupItem>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
