using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Domain.Enums;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.PagedMedicalCenters;

// ── Query Params ────────────────────────────────────────────────
public record PagedMedicalCentersParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "name",
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedMedicalCenterItem(
    Guid Id,
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt);


// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedMedicalCentersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    // Whitelist de columnas permitidas para ORDER BY — previene SQL injection
    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]     = "name",
            ["type"]     = "type",
            ["address"]  = "address",
            ["isactive"] = "is_active"
        };

    public PagedMedicalCentersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedMedicalCenterItem>> HandleAsync(
        PagedMedicalCentersParams queryParams,
        CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"SELECT COUNT(*) FROM medical_centers {where}";
        var dataSql  = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = await connection.QueryAsync<PagedMedicalCenterItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedMedicalCenterItem>(items, page, pageSize, totalCount);
    }

    // ── Helpers privados — cada uno hace una sola cosa ──────────

    private static DynamicParameters BuildParameters(
        PagedMedicalCentersParams queryParams, int pageSize, int offset)
    {
        var p = new DynamicParameters();
        p.Add("PageSize", pageSize);
        p.Add("Offset", offset);

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
            p.Add("Search", $"%{queryParams.Search.Trim()}%");

        return p;
    }

    private static string BuildWhereClause(PagedMedicalCentersParams queryParams)
    {
        if (string.IsNullOrWhiteSpace(queryParams.Search))
            return string.Empty;

        return """
               WHERE name    ILIKE @Search
                  OR address ILIKE @Search
                  OR type    ILIKE @Search
               """;
    }

    private static string BuildOrderByClause(PagedMedicalCentersParams queryParams)
    {
        // Si el cliente envía un campo no permitido, cae al default seguro "name"
        var column    = AllowedSortColumns.GetValueOrDefault(queryParams.SortBy ?? "name", "name");
        var direction = queryParams.SortDesc ? "DESC" : "ASC";

        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT id         AS Id,
               name       AS Name,
               type       AS Type,
               address    AS Address,
               phone      AS Phone,
               is_active  AS IsActive,
               latitude   AS Latitude,
               longitude  AS Longitude,
               created_at AS CreatedAt,
               updated_at AS UpdatedAt
        FROM medical_centers
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
