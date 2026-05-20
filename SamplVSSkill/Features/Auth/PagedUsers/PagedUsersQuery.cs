using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.PagedUsers;

// ── Query Params ────────────────────────────────────────────────
public record PagedUsersParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "created_at",
    bool SortDesc = false);

// ── Response Item ───────────────────────────────────────────────
public record PagedUserItem(
    string Id,
    string Email,
    string Name,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? PhotoUrl,
    Guid? InsurerId,
    string? InsurerName,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class PagedUsersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]         = "u.\"Name\"",
            ["lastname"]     = "u.\"LastName\"",
            ["email"]        = "u.\"Email\"",
            ["insurername"]  = "i.name",
            ["emailconfirmed"] = "u.\"EmailConfirmed\"",
            ["created_at"]   = "u.\"NormalizedEmail\"",   // AspNetUsers has no created_at; fallback to email sort
        };

    public PagedUsersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedUserItem>> HandleAsync(
        PagedUsersParams queryParams, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"""
            SELECT COUNT(*)
            FROM "AspNetUsers" u
            LEFT JOIN insurers i ON u."insurer_id" = i.id
            {where}
            """;

        var dataSql = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = await connection.QueryAsync<PagedUserItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return new PaginatedResult<PagedUserItem>(items, page, pageSize, totalCount);
    }

    private static DynamicParameters BuildParameters(PagedUsersParams p, int pageSize, int offset)
    {
        var dp = new DynamicParameters();
        dp.Add("PageSize", pageSize);
        dp.Add("Offset", offset);
        if (!string.IsNullOrWhiteSpace(p.Search))
            dp.Add("Search", $"%{p.Search.Trim()}%");
        return dp;
    }

    private static string BuildWhereClause(PagedUsersParams p) =>
        string.IsNullOrWhiteSpace(p.Search) ? string.Empty
            : """
              WHERE u."Email"    ILIKE @Search
                 OR u."Name"     ILIKE @Search
                 OR u."LastName" ILIKE @Search
                 OR i.name       ILIKE @Search
              """;

    private static string BuildOrderByClause(PagedUsersParams p)
    {
        var column    = AllowedSortColumns.GetValueOrDefault(p.SortBy ?? "name", "u.\"Name\"");
        var direction = p.SortDesc ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy) => $"""
        SELECT u."Id"             AS Id,
               u."Email"          AS Email,
               u."name"           AS Name,
               u."last_name"       AS LastName,
               u."PhoneNumber"    AS PhoneNumber,
               u."date_of_birth"  AS DateOfBirth,
               u."PhotoUrl"      AS PhotoUrl,
               u."insurer_id"     AS InsurerId,
               i.name             AS InsurerName,
               u."EmailConfirmed" AS EmailConfirmed,
               (u."LockoutEnd" IS NOT NULL AND u."LockoutEnd" > NOW()) AS IsLockedOut,
               NOW()              AS CreatedAt
        FROM "AspNetUsers" u
        LEFT JOIN insurers i ON u."insurer_id" = i.id
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
