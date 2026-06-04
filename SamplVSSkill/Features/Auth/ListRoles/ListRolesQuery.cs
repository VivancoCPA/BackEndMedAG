using Dapper;
using SamplVSSkill.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.ListRoles;

// ── Response ────────────────────────────────────────────────────
public record ListRolesResponse(string Id, string Name, string Description, bool IsActive, DateTime CreatedAt, int AssignedUsersCount);

// ── Query Handler ───────────────────────────────────────────────
public class ListRolesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListRolesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListRolesResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT r."Id"                   AS Id,
                   r."Name"                 AS Name,
                   r."Description"          AS Description,
                   r."IsActive"             AS IsActive,
                   r."CreatedAt"            AS CreatedAt,
                   COUNT(ur."UserId")::int  AS AssignedUsersCount
            FROM "AspNetRoles" r
            LEFT JOIN "AspNetUserRoles" ur ON r."Id" = ur."RoleId"
            GROUP BY r."Id", r."Name", r."Description", r."IsActive", r."CreatedAt"
            ORDER BY r."Name"
            """;

        var roles = await connection.QueryAsync<ListRolesResponse>(
            new CommandDefinition(sql, cancellationToken: ct));

        return roles;
    }
}

