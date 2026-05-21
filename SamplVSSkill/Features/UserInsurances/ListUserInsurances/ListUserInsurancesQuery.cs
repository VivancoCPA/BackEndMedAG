using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.UserInsurances.ListUserInsurances;

// ── Response ────────────────────────────────────────────────────
public record UserInsuranceItem(
    Guid InsurerId,
    string InsurerName,
    string? InsurerPhone,
    string? InsurerEmail,
    string? LogoUrl,
    DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListUserInsurancesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;
    public ListUserInsurancesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IResult> HandleAsync(string userId, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT  i.id           AS InsurerId,
                    i.name         AS InsurerName,
                    i.phone        AS InsurerPhone,
                    i.email        AS InsurerEmail,
                    i.logo_url     AS LogoUrl,
                    ui."CreatedAt" AS CreatedAt
            FROM user_insurances ui
            INNER JOIN insurers i ON ui.insurer_id = i.id
            WHERE ui.user_id = @UserId
            ORDER BY i.name
            """;

        var items = await connection.QueryAsync<UserInsuranceItem>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

        return Results.Ok(items);
    }
}
