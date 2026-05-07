using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Specialties.PagedSpecialties;

// ── Query Params ────────────────────────────────────────────────
public record PagedSpecialtiesParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedSpecialtyItem(int Id, string Name, bool IsActive, DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedSpecialtiesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]       = "name",
            ["isactive"]   = "is_active",
            ["created_at"] = "created_at"   // default sort para primera llamada
        };

    public PagedSpecialtiesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedSpecialtyItem>> HandleAsync(
        PagedSpecialtiesParams queryParams, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"SELECT COUNT(*) FROM specialties {where}";
        var dataSql  = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = await connection.QueryAsync<PagedSpecialtyItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedSpecialtyItem>(items, page, pageSize, totalCount);
    }

    private static DynamicParameters BuildParameters(
        PagedSpecialtiesParams p, int pageSize, int offset)
    {
        var dp = new DynamicParameters();
        dp.Add("PageSize", pageSize);
        dp.Add("Offset", offset);
        if (!string.IsNullOrWhiteSpace(p.Search))
            dp.Add("Search", $"%{p.Search.Trim()}%");
        return dp;
    }

    private static string BuildWhereClause(PagedSpecialtiesParams p) =>
        string.IsNullOrWhiteSpace(p.Search) ? string.Empty
            : "WHERE name ILIKE @Search";

    private static string BuildOrderByClause(PagedSpecialtiesParams p)
    {
        // Sin SortBy → orden por fecha de creación (más recientes primero por default)
        var column    = AllowedSortColumns.GetValueOrDefault(p.SortBy ?? "created_at", "created_at");
        var direction = p.SortDesc ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT id         AS Id,
               name       AS Name,
               is_active  AS IsActive,
               created_at AS CreatedAt
        FROM specialties
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
