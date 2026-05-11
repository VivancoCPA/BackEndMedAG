using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.CenterTypes.LookupCenterTypes;

// ── Query Handler (Dapper) ──────────────────────────────────────
public class LookupCenterTypesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public LookupCenterTypesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<LookupItem>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id   AS Id,
                   name AS Name
            FROM centers_type
            WHERE is_active = true
            ORDER BY name
            """;

        return await connection.QueryAsync<LookupItem>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
