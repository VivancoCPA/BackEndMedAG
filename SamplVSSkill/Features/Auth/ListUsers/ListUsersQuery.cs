using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.ListUsers;

// ── Response ────────────────────────────────────────────────────
public record ListUsersResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? PhotoUrl,
    Guid? InsurerId,
    string? InsurerName,
    bool EmailConfirmed,
    bool IsLockedOut);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListUsersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListUsersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListUsersResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT u."Id"             AS Id,
                   u."Email"          AS Email,
                   u."name"           AS Name,
                   u."last_name"       AS LastName,
                   u."PhoneNumber"    AS PhoneNumber,
                   u."date_of_birth"  AS DateOfBirth,
                   u."PhotoUrl"      AS PhotoUrl,
                   u."insurer_id"     AS InsurerId,
                   i.name             AS InsurerName,
                   u."EmailConfirmed" AS EmailConfirmed,
                   (u."LockoutEnd" IS NOT NULL AND u."LockoutEnd" > NOW()) AS IsLockedOut
            FROM "AspNetUsers" u
            LEFT JOIN insurers i ON u."insurer_id" = i.id
            ORDER BY u."name", u."last_name"
            """;

        return await connection.QueryAsync<ListUsersResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
