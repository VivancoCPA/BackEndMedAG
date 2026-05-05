using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.GetUserRoles;

// ── Response ────────────────────────────────────────────────────
public record GetUserRolesResponse(string UserId, string Email, IList<string> Roles);

// ── Query Handler ───────────────────────────────────────────────
public class GetUserRolesQueryHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public GetUserRolesQueryHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var roles = await _userManager.GetRolesAsync(user);
        return Results.Ok(new GetUserRolesResponse(user.Id, user.Email!, roles));
    }
}
