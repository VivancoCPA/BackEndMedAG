using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyMemberships.ListFamilyMemberships;

// ── Response ────────────────────────────────────────────────────
public record FamilyMembershipItem(
    int Id,
    string UserId,
    string UserEmail,
    string UserName,
    string UserLastName,
    string? UserPhotoUrl,
    bool IsAdmin,
    string? Relationship);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListFamilyMembershipsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;
    public ListFamilyMembershipsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IResult> HandleAsync(Guid familyGroupId, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Verificar si el grupo familiar existe
        const string checkSql = @"SELECT EXISTS(SELECT 1 FROM family_groups WHERE id = @FamilyGroupId)";
        var groupExists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(checkSql, new { FamilyGroupId = familyGroupId }, cancellationToken: ct));

        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        const string sql = """
            SELECT  fm.id             AS Id,
                    fm.user_id        AS UserId,
                    u."Email"         AS UserEmail,
                    u."Name"          AS UserName,
                    u."LastName"      AS UserLastName,
                    u."PhotoUrl"      AS UserPhotoUrl,
                    fm.is_admin       AS IsAdmin,
                    fm.relationship   AS Relationship
            FROM family_memberships fm
            INNER JOIN "AspNetUsers" u ON fm.user_id = u."Id"
            WHERE fm.family_group_id = @FamilyGroupId
            ORDER BY u."Name", u."LastName"
            """;

        var items = await connection.QueryAsync<FamilyMembershipItem>(
            new CommandDefinition(sql, new { FamilyGroupId = familyGroupId }, cancellationToken: ct));

        return Results.Ok(items);
    }
}
