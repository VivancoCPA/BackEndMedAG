using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.GetUser;

// ── Response ────────────────────────────────────────────────────
public record GetUserResponse(
    string Id,
    string Email,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    IList<string> Roles,
    IList<string> Claims);

// ── Query Handler ───────────────────────────────────────────────
public class GetUserQueryHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public GetUserQueryHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var roles  = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var now = DateTimeOffset.UtcNow;

        return Results.Ok(new GetUserResponse(
            Id:             user.Id,
            Email:          user.Email!,
            EmailConfirmed: user.EmailConfirmed,
            IsLockedOut:    user.LockoutEnd != null && user.LockoutEnd > now,
            LockoutEnd:     user.LockoutEnd,
            Roles:          roles,
            Claims:         claims.Select(c => $"{c.Type}:{c.Value}").ToList()));
    }
}
