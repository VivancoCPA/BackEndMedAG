using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Insurers.PagedInsurers;

// ── Query Params ────────────────────────────────────────────────
public record PagedInsurersParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "name",
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedInsurerItem(
    Guid Id, string Name, string Phone, string Email,
    string? PersonInCharge, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedInsurersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]     = "name",
            ["email"]    = "email",
            ["isactive"] = "is_active"
        };

    public PagedInsurersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedInsurerItem>> HandleAsync(
        PagedInsurersParams queryParams, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"SELECT COUNT(*) FROM insurers {where}";
        var dataSql  = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = await connection.QueryAsync<PagedInsurerItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedInsurerItem>(items, page, pageSize, totalCount);
    }

    private static DynamicParameters BuildParameters(
        PagedInsurersParams p, int pageSize, int offset)
    {
        var dp = new DynamicParameters();
        dp.Add("PageSize", pageSize);
        dp.Add("Offset", offset);
        if (!string.IsNullOrWhiteSpace(p.Search))
            dp.Add("Search", $"%{p.Search.Trim()}%");
        return dp;
    }

    private static string BuildWhereClause(PagedInsurersParams p) =>
        string.IsNullOrWhiteSpace(p.Search) ? string.Empty
            : """
              WHERE name  ILIKE @Search
                 OR email ILIKE @Search
              """;

    private static string BuildOrderByClause(PagedInsurersParams p)
    {
        var column    = AllowedSortColumns.GetValueOrDefault(p.SortBy ?? "name", "name");
        var direction = p.SortDesc ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT id               AS Id,
               name             AS Name,
               phone            AS Phone,
               email            AS Email,
               person_in_charge AS PersonInCharge,
               is_active        AS IsActive,
               created_at       AS CreatedAt,
               updated_at       AS UpdatedAt
        FROM insurers
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
