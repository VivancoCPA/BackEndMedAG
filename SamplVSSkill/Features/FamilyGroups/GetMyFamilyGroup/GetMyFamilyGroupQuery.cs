using Dapper;
using SamplVSSkill.Infrastructure.Persistence;
using SamplVSSkill.Features.FamilyGroups.ListFamilyGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyGroups.GetMyFamilyGroup;

// ── Database Mappings (Internal) ────────────────────────────────
internal record FamilyGroupMemberDb(
    int Id,
    Guid FamilyGroupId,
    string UserId,
    string Email,
    string Name,
    string LastName,
    string? PhotoUrl,
    bool IsAdmin,
    string? Relationship,
    bool IsActive);

internal record FamilyGroupExtraMemberDb(
    int Id,
    Guid FamilyGroupId,
    string FullName,
    string IdType,
    string? PhotoUrl,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

internal record ListFamilyGroupsResponseInternal(
    Guid Id,
    string Name,
    string? UserId,
    string? OwnerName,
    string? PhotoUrl,
    bool IsActive,
    DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class GetMyFamilyGroupQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;
    public GetMyFamilyGroupQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListFamilyGroupsResponse>> HandleAsync(string userId, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT fg.id           AS Id,
                   fg.name         AS Name,
                   fg.user_id      AS UserId,
                   (u."Name" || ' ' || u."LastName") AS OwnerName,
                   fg.photo_url    AS PhotoUrl,
                   fg.is_active    AS IsActive,
                   fg.created_at   AS CreatedAt
            FROM family_groups fg
            LEFT JOIN "AspNetUsers" u ON fg.user_id = u."Id"
            WHERE fg.id IN (SELECT family_group_id FROM family_memberships WHERE user_id = @UserId)
              AND (fg.user_id <> @UserId OR fg.user_id IS NULL)
            ORDER BY fg.name
            """;

        var groups = (await connection.QueryAsync<ListFamilyGroupsResponseInternal>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct))).ToList();

        if (!groups.Any())
        {
            return Enumerable.Empty<ListFamilyGroupsResponse>();
        }

        var groupIds = groups.Select(g => g.Id).ToList();

        // 1. Obtener miembros registrados (family_memberships)
        const string membersSql = """
            SELECT  fm.id             AS Id,
                    fm.family_group_id AS FamilyGroupId,
                    fm.user_id        AS UserId,
                    u."Email"         AS Email,
                    u."Name"          AS Name,
                    u."LastName"      AS LastName,
                    u."PhotoUrl"      AS PhotoUrl,
                    fm.is_admin       AS IsAdmin,
                    fm.relationship   AS Relationship,
                    fm.is_active      AS IsActive
            FROM family_memberships fm
            INNER JOIN "AspNetUsers" u ON fm.user_id = u."Id"
            WHERE fm.family_group_id = ANY(@GroupIds)
            ORDER BY u."Name", u."LastName"
            """;

        // 2. Obtener miembros extra (family_extra_memberships)
        const string extraMembersSql = """
            SELECT  fem.id              AS Id,
                    fem.family_group_id AS FamilyGroupId,
                    fem.full_name       AS FullName,
                    fem.id_type         AS IdType,
                    fem.photo_url       AS PhotoUrl,
                    fem.description     AS Description,
                    fem.is_active       AS IsActive,
                    fem.created_at      AS CreatedAt
            FROM family_extra_memberships fem
            WHERE fem.family_group_id = ANY(@GroupIds)
            ORDER BY fem.full_name
            """;

        var membersDb = (await connection.QueryAsync<FamilyGroupMemberDb>(
            new CommandDefinition(membersSql, new { GroupIds = groupIds }, cancellationToken: ct))).ToList();

        var extraMembersDb = (await connection.QueryAsync<FamilyGroupExtraMemberDb>(
            new CommandDefinition(extraMembersSql, new { GroupIds = groupIds }, cancellationToken: ct))).ToList();

        var membersByGroup = membersDb.GroupBy(m => m.FamilyGroupId)
                                      .ToDictionary(g => g.Key, g => g.Select(m => new FamilyGroupMemberDto(
                                          m.Id, m.UserId, m.Email, m.Name, m.LastName, m.PhotoUrl, m.IsAdmin, m.Relationship, m.IsActive
                                      )).ToList());

        var extraMembersByGroup = extraMembersDb.GroupBy(m => m.FamilyGroupId)
                                                .ToDictionary(g => g.Key, g => g.Select(m => new FamilyGroupExtraMemberDto(
                                                    m.Id, m.FullName, m.IdType, m.PhotoUrl, m.Description, m.IsActive, m.CreatedAt
                                                )).ToList());

        return groups.Select(g => new ListFamilyGroupsResponse(
            g.Id,
            g.Name,
            g.UserId,
            g.OwnerName,
            g.PhotoUrl,
            g.IsActive,
            g.CreatedAt,
            membersByGroup.GetValueOrDefault(g.Id) ?? Enumerable.Empty<FamilyGroupMemberDto>(),
            extraMembersByGroup.GetValueOrDefault(g.Id) ?? Enumerable.Empty<FamilyGroupExtraMemberDto>()
        )).ToList();
    }
}
