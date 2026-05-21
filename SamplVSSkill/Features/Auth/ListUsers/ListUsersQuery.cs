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
    Guid? FamilyGroupId,
    string? FamilyGroupName,
    IReadOnlyList<UserInsuranceSummary> Insurances);

// ── Private flat row (for initial user query) ──────────────────
file record ListUserFlat(
    string Id, string Email, string Name, string LastName,
    string? PhoneNumber, DateTime? DateOfBirth, string? PhotoUrl,
    string? Address, bool EmailConfirmed, bool IsLockedOut,
    Guid? FamilyGroupId, string? FamilyGroupName);

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

    public async Task<IEnumerable<ListUsersResponse>> HandleAsync(CancellationToken ct)
    {
        const string sql = """
            -- 1) usuarios con grupo familiar
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
                   fg.id              AS FamilyGroupId,
                   fg.name            AS FamilyGroupName
            FROM "AspNetUsers" u
            LEFT JOIN family_groups fg ON fg.user_id = u."Id"
            ORDER BY u."Name", u."LastName";

            -- 2) aseguradoras de todos los usuarios
            SELECT ui.user_id    AS UserId,
                   i.id          AS InsurerId,
                   i.name        AS InsurerName,
                   i.phone       AS InsurerPhone,
                   i.email       AS InsurerEmail,
                   i.logo_url    AS LogoUrl
            FROM user_insurances ui
            INNER JOIN insurers i ON ui.insurer_id = i.id;
            """;

        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, cancellationToken: ct));

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
            u.EmailConfirmed, u.IsLockedOut,
            u.FamilyGroupId, u.FamilyGroupName,
            insMap.TryGetValue(u.Id, out var ins) ? ins : Array.Empty<UserInsuranceSummary>()));
    }
}
