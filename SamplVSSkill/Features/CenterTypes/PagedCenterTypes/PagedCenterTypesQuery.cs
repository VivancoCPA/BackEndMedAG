using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.CenterTypes.PagedCenterTypes;

// ── Query Params ────────────────────────────────────────────────
public record PagedCenterTypesParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,       // null → default: created_at
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedCenterTypeItem(int Id, string Name, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedCenterTypesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]       = "name",
            ["isactive"]   = "is_active",
            ["created_at"] = "created_at",
            ["updated_at"] = "updated_at"
        };

    public PagedCenterTypesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedCenterTypeItem>> HandleAsync(
        PagedCenterTypesParams queryParams, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"SELECT COUNT(*) FROM centers_type {where}";
        var dataSql  = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = await connection.QueryAsync<PagedCenterTypeItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedCenterTypeItem>(items, page, pageSize, (int)totalCount);
    }

    private static DynamicParameters BuildParameters(
        PagedCenterTypesParams p, int pageSize, int offset)
    {
        var dp = new DynamicParameters();
        dp.Add("PageSize", pageSize);
        dp.Add("Offset", offset);
        if (!string.IsNullOrWhiteSpace(p.Search))
            dp.Add("Search", $"%{p.Search.Trim()}%");
        return dp;
    }

    private static string BuildWhereClause(PagedCenterTypesParams p) =>
        string.IsNullOrWhiteSpace(p.Search) ? string.Empty
            : "WHERE name ILIKE @Search";

    private static string BuildOrderByClause(PagedCenterTypesParams p)
    {
        // Sin SortBy → orden por fecha de creación
        var column    = AllowedSortColumns.GetValueOrDefault(p.SortBy ?? "created_at", "created_at");
        var direction = p.SortDesc ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT id         AS Id,
               name       AS Name,
               is_active  AS IsActive,
               created_at AS CreatedAt,
               updated_at AS UpdatedAt
        FROM centers_type
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
