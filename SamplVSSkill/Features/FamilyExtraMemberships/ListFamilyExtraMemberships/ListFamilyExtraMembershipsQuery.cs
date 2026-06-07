using Dapper;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.ListFamilyExtraMemberships;

// ── Response Item ───────────────────────────────────────────────
public record FamilyExtraMembershipItem(
    int Id,
    string FullName,
    string IdType,
    string? PhotoUrl,
    Guid FamilyGroupId,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListFamilyExtraMembershipsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;
    public ListFamilyExtraMembershipsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IResult> HandleAsync(Guid familyGroupId, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        // 1. Verificar si el grupo familiar existe
        const string checkSql = "SELECT EXISTS(SELECT 1 FROM family_groups WHERE id = @FamilyGroupId)";
        var groupExists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(checkSql, new { FamilyGroupId = familyGroupId }, cancellationToken: ct));

        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        // 2. Consultar miembros extra
        const string sql = """
            SELECT  id              AS Id,
                    full_name       AS FullName,
                    id_type         AS IdType,
                    photo_url       AS PhotoUrl,
                    family_group_id AS FamilyGroupId,
                    description     AS Description,
                    is_active       AS IsActive,
                    created_at      AS CreatedAt
            FROM family_extra_memberships
            WHERE family_group_id = @FamilyGroupId
            ORDER BY full_name
            """;

        var items = await connection.QueryAsync<FamilyExtraMembershipItem>(
            new CommandDefinition(sql, new { FamilyGroupId = familyGroupId }, cancellationToken: ct));

        return Results.Ok(items);
    }
}
