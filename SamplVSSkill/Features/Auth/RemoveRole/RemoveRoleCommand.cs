using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.RemoveRole;

// ── Response ────────────────────────────────────────────────────
public record RemoveRoleResponse(string UserId, string Email, IList<string> RemainingRoles);

// ── Command Handler ─────────────────────────────────────────────
public class RemoveRoleCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public RemoveRoleCommandHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, string roleName, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        if (!await _userManager.IsInRoleAsync(user, roleName))
            return Results.NotFound($"El usuario no tiene el rol '{roleName}'.");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        var remaining = await _userManager.GetRolesAsync(user);
        return Results.Ok(new RemoveRoleResponse(user.Id, user.Email!, remaining));
    }
}
