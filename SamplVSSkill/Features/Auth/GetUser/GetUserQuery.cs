using Dapper;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.GetUser;

// ── Response ────────────────────────────────────────────────────
public record GetUserResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    DateTime? DateOfBirth,
    bool EmailConfirmed,
    bool IsLockedOut,
    bool PasswordConfirmed,
    DateTimeOffset? LockoutEnd,
    DateTime? LastAccess,
    string? PhotoUrl,
    IList<string> Roles,
    IList<string> Claims);

// ── Query Handler ───────────────────────────────────────────────
public class GetUserQueryHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly DapperConnectionFactory _connectionFactory;

    public GetUserQueryHandler(UserManager<AppUser> userManager, DapperConnectionFactory connectionFactory)
    {
        _userManager = userManager;
        _connectionFactory = connectionFactory;
    }

    public async Task<IResult> HandleAsync(
        string userId, string currentUserId, bool bypassScope, CancellationToken ct)
    {
        if (!bypassScope && userId != currentUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var inScope = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    "SELECT EXISTS(SELECT 1 FROM user_scope WHERE user_id_admin = @CurrentUserId AND user_id = @UserId)",
                    new { CurrentUserId = currentUserId, UserId = userId },
                    cancellationToken: ct));

            if (!inScope)
            {
                return Results.Forbid();
            }
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var roles  = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var now = DateTimeOffset.UtcNow;

        return Results.Ok(new GetUserResponse(
            Id:             user.Id,
            Email:          user.Email!,
            Name:           user.Name,
            LastName:       user.LastName,
            DateOfBirth:    user.DateOfBirth,
            EmailConfirmed: user.EmailConfirmed,
            IsLockedOut:    user.LockoutEnd != null && user.LockoutEnd > now,
            LockoutEnd:     user.LockoutEnd,
            PasswordConfirmed: user.PasswordConfirmed,
            LastAccess:     user.LastAccess,
            PhotoUrl:       user.PhotoUrl,
            Roles:          roles,
            Claims:         claims.Select(c => $"{c.Type}:{c.Value}").ToList()));
    }
}
