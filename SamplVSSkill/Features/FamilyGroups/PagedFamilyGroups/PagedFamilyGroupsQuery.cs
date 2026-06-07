using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyGroups.PagedFamilyGroups;

// ── Query Params ────────────────────────────────────────────────
public record PagedFamilyGroupsParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "name",
    bool SortDesc = false);

// ── Response Item DTOs ──────────────────────────────────────────
public record FamilyGroupMemberDto(
    int Id,
    string UserId,
    string Email,
    string Name,
    string LastName,
    string? PhotoUrl,
    bool IsAdmin,
    string? Relationship,
    bool IsActive);

public record FamilyGroupExtraMemberDto(
    int Id,
    string FullName,
    string IdType,
    string? PhotoUrl,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

public record PagedFamilyGroupItem(
    Guid Id,
    string Name,
    string? UserId,
    string? OwnerName,
    string? PhotoUrl,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<FamilyGroupMemberDto> Members,
    IEnumerable<FamilyGroupExtraMemberDto> ExtraMembers);

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

internal record PagedFamilyGroupItemInternal(
    Guid Id,
    string Name,
    string? UserId,
    string? OwnerName,
    string? PhotoUrl,
    bool IsActive,
    DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedFamilyGroupsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]      = "fg.name",
            ["ownername"] = "OwnerName",
            ["isactive"]  = "fg.is_active",
            ["createdat"] = "fg.created_at"
        };

    public PagedFamilyGroupsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedFamilyGroupItem>> HandleAsync(
        PagedFamilyGroupsParams queryParams, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"""
            SELECT COUNT(*)
            FROM family_groups fg
            LEFT JOIN "AspNetUsers" u ON fg.user_id = u."Id"
            {where}
            """;

        var dataSql = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var groups = (await connection.QueryAsync<PagedFamilyGroupItemInternal>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct))).ToList();

        if (!groups.Any())
        {
            return new PaginatedResult<PagedFamilyGroupItem>(
                Enumerable.Empty<PagedFamilyGroupItem>(), page, pageSize, totalCount);
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

        var enrichedGroups = groups.Select(g => new PagedFamilyGroupItem(
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

        return new PaginatedResult<PagedFamilyGroupItem>(enrichedGroups, page, pageSize, totalCount);
    }

    private static DynamicParameters BuildParameters(PagedFamilyGroupsParams p, int pageSize, int offset)
    {
        var dp = new DynamicParameters();
        dp.Add("PageSize", pageSize);
        dp.Add("Offset", offset);
        if (!string.IsNullOrWhiteSpace(p.Search))
            dp.Add("Search", $"%{p.Search.Trim()}%");
        return dp;
    }

    private static string BuildWhereClause(PagedFamilyGroupsParams p) =>
        string.IsNullOrWhiteSpace(p.Search) ? string.Empty
            : """
              WHERE fg.name ILIKE @Search
                 OR (u."Name" || ' ' || u."LastName") ILIKE @Search
              """;

    private static string BuildOrderByClause(PagedFamilyGroupsParams p)
    {
        var column    = AllowedSortColumns.GetValueOrDefault(p.SortBy ?? "name", "fg.name");
        var direction = p.SortDesc ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT fg.id           AS Id,
               fg.name         AS Name,
               fg.user_id      AS UserId,
               (u."Name" || ' ' || u."LastName") AS OwnerName,
               fg.photo_url    AS PhotoUrl,
               fg.is_active    AS IsActive,
               fg.created_at   AS CreatedAt
        FROM family_groups fg
        LEFT JOIN "AspNetUsers" u ON fg.user_id = u."Id"
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
