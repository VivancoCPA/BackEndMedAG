using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyGroups.PagedFamilyGroups;

// ── Query Params ────────────────────────────────────────────────
public record PagedFamilyGroupsParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "name",
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedFamilyGroupItem(
    Guid Id, string Name, string? UserId, string? OwnerName,
    string? PhotoUrl, bool IsActive, DateTime CreatedAt);

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

        var items = await connection.QueryAsync<PagedFamilyGroupItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedFamilyGroupItem>(items, page, pageSize, totalCount);
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
                 OR (u."name" || ' ' || u."last_name") ILIKE @Search
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
               (u."name" || ' ' || u."last_name") AS OwnerName,
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
