using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.PagedMedicalCenters;

// ── Query Params ────────────────────────────────────────────────
public record PagedMedicalCentersParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "created_at",
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedMedicalCenterItem(
    Guid Id,
    string Name,
    int? TypeId,
    string? TypeName,
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
    // Todas las referencias usan el alias "mc." para evitar ambigüedad con el JOIN
    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]       = "mc.name",
            ["type"]       = "ct.name",
            ["address"]    = "mc.address",
            ["isactive"]   = "mc.is_active",
            ["created_at"] = "mc.created_at",
            ["updated_at"] = "mc.updated_at"
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

        var countSql = BuildCountSql(where);
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

        // Búsqueda sobre nombre del centro y nombre del tipo (via JOIN)
        return """
               WHERE mc.name ILIKE @Search
                  OR mc.address ILIKE @Search
                  OR ct.name    ILIKE @Search
               """;
    }

    private static string BuildOrderByClause(PagedMedicalCentersParams queryParams)
    {
        // Sin SortBy → orden por fecha de creación
        var column    = AllowedSortColumns.GetValueOrDefault(queryParams.SortBy ?? "created_at", "mc.created_at");
        var direction = queryParams.SortDesc ? "DESC" : "ASC";

        return $"ORDER BY {column} {direction}";
    }

    private static string BuildCountSql(string where) => $"""
        SELECT COUNT(*)
        FROM medical_centers mc
        LEFT JOIN centers_type ct ON ct.id = mc.type_id
        {where}
        """;

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT mc.id         AS Id,
               mc.name       AS Name,
               mc.type_id    AS TypeId,
               ct.name       AS TypeName,
               mc.address    AS Address,
               mc.phone      AS Phone,
               mc.is_active  AS IsActive,
               mc.latitude   AS Latitude,
               mc.longitude  AS Longitude,
               mc.created_at AS CreatedAt,
               mc.updated_at AS UpdatedAt
        FROM medical_centers mc
        LEFT JOIN centers_type ct ON ct.id = mc.type_id
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
