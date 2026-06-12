using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.ListUsers;

// ── Shared sub-record ───────────────────────────────────────────
public record UserInsuranceSummary(
    Guid InsurerId, string InsurerName,
    string? InsurerPhone, string? InsurerEmail, string? LogoUrl);

// ── Response ────────────────────────────────────────────────────
public record ListUsersResponse(
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
    DateTime? LastAccess,
    bool PasswordConfirmed,
    IReadOnlyList<UserInsuranceSummary> Insurances);

// ── Private flat row (for initial user query) ──────────────────
file record ListUserFlat(
    string Id, string Email, string Name, string LastName,
    string? PhoneNumber, DateTime? DateOfBirth, string? PhotoUrl,
    string? Address, bool EmailConfirmed, bool IsLockedOut, DateTime? LastAccess,
    bool PasswordConfirmed);

// ── Private insurance row ───────────────────────────────────────
file record InsuranceRow(
    string UserId, Guid InsurerId, string InsurerName,
    string? InsurerPhone, string? InsurerEmail, string? LogoUrl);

// ── Query Handler (Dapper, two-query approach) ──────────────────
public class ListUsersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListUsersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListUsersResponse>> HandleAsync(
        string currentUserId, bool bypassScope, CancellationToken ct)
    {
        var whereClause = bypassScope ? "" : "WHERE u.\"Id\" IN (SELECT user_id FROM user_scope WHERE user_id_admin = @CurrentUserId)";
        var insuranceFilter = bypassScope ? "" : "WHERE ui.user_id IN (SELECT user_id FROM user_scope WHERE user_id_admin = @CurrentUserId)";

        var sql = $"""
            -- 1) usuarios
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
                   u."LastAccess"     AS LastAccess,
                   u."PasswordConfirmed" AS PasswordConfirmed
            FROM "AspNetUsers" u
            {whereClause}
            ORDER BY u."Name", u."LastName";

            -- 2) aseguradoras de todos los usuarios
            SELECT ui.user_id    AS UserId,
                   i.id          AS InsurerId,
                   i.name        AS InsurerName,
                   i.phone       AS InsurerPhone,
                   i.email       AS InsurerEmail,
                   i.logo_url    AS LogoUrl
            FROM user_insurances ui
            INNER JOIN insurers i ON ui.insurer_id = i.id
            {insuranceFilter};
            """;

        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { CurrentUserId = currentUserId }, cancellationToken: ct));

        var users      = (await multi.ReadAsync<ListUserFlat>()).ToList();
        var insMap     = (await multi.ReadAsync<InsuranceRow>())
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g =>
                (IReadOnlyList<UserInsuranceSummary>)g
                    .Select(r => new UserInsuranceSummary(r.InsurerId, r.InsurerName, r.InsurerPhone, r.InsurerEmail, r.LogoUrl))
                    .ToList());

        return users.Select(u => new ListUsersResponse(
            u.Id, u.Email, u.Name, u.LastName,
            u.PhoneNumber, u.DateOfBirth, u.PhotoUrl, u.Address,
            u.EmailConfirmed, u.IsLockedOut, u.LastAccess, u.PasswordConfirmed,
            insMap.TryGetValue(u.Id, out var ins) ? ins : Array.Empty<UserInsuranceSummary>()));
    }
}
