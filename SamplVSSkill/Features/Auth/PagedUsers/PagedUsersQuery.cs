using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.PagedUsers;

// ── Query Params ────────────────────────────────────────────────
public record PagedUsersParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "name",
    bool SortDesc = false);

// ── Sub-record for insurance items ──────────────────────────────
public record PagedUserInsurance(
    Guid InsurerId, string InsurerName,
    string? InsurerPhone, string? InsurerEmail, string? LogoUrl);

// ── Response Item ───────────────────────────────────────────────
public record PagedUserItem(
    string Id,
    string Email,
    string Name,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? PhotoUrl,
    string? Address,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime CreatedAt,
    Guid? FamilyGroupId,
    string? FamilyGroupName,
    IReadOnlyList<PagedUserInsurance> Insurances);

// ── Private flat row (without Insurances) ──────────────────────
file record PagedUserFlat(
    string Id, string Email, string Name, string LastName,
    string? PhoneNumber, DateTime? DateOfBirth, string? PhotoUrl,
    string? Address, bool EmailConfirmed, bool IsLockedOut,
    DateTime CreatedAt, Guid? FamilyGroupId, string? FamilyGroupName);

// ── Private insurance row ───────────────────────────────────────
file record PagedInsuranceRow(
    string UserId, Guid InsurerId, string InsurerName,
    string? InsurerPhone, string? InsurerEmail, string? LogoUrl);

// ── Query Handler (Dapper, two-query approach) ──────────────────
public class PagedUsersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]           = "u.\"Name\"",
            ["lastname"]       = "u.\"LastName\"",
            ["email"]          = "u.\"Email\"",
            ["emailconfirmed"] = "u.\"EmailConfirmed\"",
            ["createdat"]      = "u.\"CreatedAt\"",
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
            LEFT JOIN family_groups fg ON fg.user_id = u."Id"
            {where}
            """;

        var dataSql = BuildDataSql(where, orderBy);

        using var connection = _connectionFactory.CreateConnection();

        // ── 1) Count ─────────────────────────────────────────────
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        // ── 2) Paged users (flat, without insurances) ─────────────
        var flatItems = (await connection.QueryAsync<PagedUserFlat>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct))).ToList();

        if (flatItems.Count == 0)
            return new PaginatedResult<PagedUserItem>([], page, pageSize, totalCount);

        // ── 3) Insurances only for users on this page ─────────────
        var userIds = flatItems.Select(u => u.Id).ToArray();

        const string insuranceSql = """
            SELECT ui.user_id    AS UserId,
                   i.id          AS InsurerId,
                   i.name        AS InsurerName,
                   i.phone       AS InsurerPhone,
                   i.email       AS InsurerEmail,
                   i.logo_url    AS LogoUrl
            FROM user_insurances ui
            INNER JOIN insurers i ON ui.insurer_id = i.id
            WHERE ui.user_id = ANY(@UserIds)
            """;

        var insMap = (await connection.QueryAsync<PagedInsuranceRow>(
            new CommandDefinition(insuranceSql, new { UserIds = userIds }, cancellationToken: ct)))
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g =>
                (IReadOnlyList<PagedUserInsurance>)g
                    .Select(r => new PagedUserInsurance(r.InsurerId, r.InsurerName, r.InsurerPhone, r.InsurerEmail, r.LogoUrl))
                    .ToList());

        // ── 4) Merge ──────────────────────────────────────────────
        var items = flatItems.Select(u => new PagedUserItem(
            u.Id, u.Email, u.Name, u.LastName,
            u.PhoneNumber, u.DateOfBirth, u.PhotoUrl, u.Address,
            u.EmailConfirmed, u.IsLockedOut, u.CreatedAt,
            u.FamilyGroupId, u.FamilyGroupName,
            insMap.TryGetValue(u.Id, out var ins) ? ins : Array.Empty<PagedUserInsurance>()));

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
                 OR fg.name      ILIKE @Search
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
               u."Name"           AS Name,
               u."LastName"       AS LastName,
               u."PhoneNumber"    AS PhoneNumber,
               u."DateOfBirth"    AS DateOfBirth,
               u."PhotoUrl"       AS PhotoUrl,
               u."Address"        AS Address,
               u."EmailConfirmed" AS EmailConfirmed,
               (u."LockoutEnd" IS NOT NULL AND u."LockoutEnd" > NOW()) AS IsLockedOut,
               u."CreatedAt"      AS CreatedAt,
               fg.id              AS FamilyGroupId,
               fg.name            AS FamilyGroupName
        FROM "AspNetUsers" u
        LEFT JOIN family_groups fg ON fg.user_id = u."Id"
        {where}
        {orderBy}
        LIMIT @PageSize OFFSET @Offset
        """;
}
