using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Insurers.ListInsurers;

// ── Response ────────────────────────────────────────────────────
public record ListInsurersResponse(
    Guid Id, string Name, string Phone, string Email,
    string? PersonInCharge, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListInsurersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListInsurersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListInsurersResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id               AS Id,
                   name             AS Name,
                   phone            AS Phone,
                   email            AS Email,
                   person_in_charge AS PersonInCharge,
                   is_active        AS IsActive,
                   created_at       AS CreatedAt,
                   updated_at       AS UpdatedAt
            FROM insurers
            ORDER BY name
            """;

        return await connection.QueryAsync<ListInsurersResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
