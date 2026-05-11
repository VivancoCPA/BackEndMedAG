using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Insurers.LookupInsurers;

// ── Query Handler (Dapper) ──────────────────────────────────────
public class LookupInsurersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public LookupInsurersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<LookupItemGuid>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id   AS Id,
                   name AS Name
            FROM insurers
            WHERE is_active = true
            ORDER BY name
            """;

        return await connection.QueryAsync<LookupItemGuid>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
