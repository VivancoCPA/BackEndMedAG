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
    string? SortBy = "name",      // columna de ordenamiento (default: name)
    bool SortDesc = false);        // true = DESC, false = ASC

// ── Response Item ───────────────────────────────────────────────
public record PagedMedicalCenterItem(
    Guid Id,
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedMedicalCentersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    // Whitelist: evita SQL injection — solo estas columnas son permitidas para ORDER BY
    private static readonly Dictionary<string, string> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"]      = "name",
        ["type"]      = "type",
        ["address"]   = "address",
        ["isactive"]  = "is_active"
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

        var parameters = new DynamicParameters();
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        // ── Search filter ────────────────────────────────────────
        var where = string.Empty;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            parameters.Add("Search", $"%{queryParams.Search.Trim()}%");
            where = """
                    WHERE name    ILIKE @Search
                       OR address ILIKE @Search
                       OR type    ILIKE @Search
                    """;
        }

        // ── Sort — usar whitelist para evitar SQL injection ───────
        var sortColumn = AllowedSortColumns.TryGetValue(queryParams.SortBy ?? "name", out var col)
            ? col
            : "name";   // fallback seguro si el cliente envía un valor no permitido

        var sortDir = queryParams.SortDesc ? "DESC" : "ASC";
        var orderBy = $"ORDER BY {sortColumn} {sortDir}";

        // ── SQL ───────────────────────────────────────────────────
        var countSql = $"SELECT COUNT(*) FROM medical_centers {where}";

        var dataSql = $"""
            SELECT id        AS Id,
                   name      AS Name,
                   type      AS Type,
                   address   AS Address,
                   phone     AS Phone,
                   is_active AS IsActive,
                   latitude  AS Latitude,
                   longitude AS Longitude
            FROM medical_centers
            {where}
            {orderBy}
            LIMIT @PageSize OFFSET @Offset
            """;

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = await connection.QueryAsync<PagedMedicalCenterItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedMedicalCenterItem>(items, page, pageSize, totalCount);
    }
}
