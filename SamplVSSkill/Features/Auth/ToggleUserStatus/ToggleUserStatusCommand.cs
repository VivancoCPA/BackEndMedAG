using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.ToggleUserStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleUserStatusResponse(
    string UserId,
    string Email,
    bool IsLockedOut,
    string Status);

// ── Command Handler ─────────────────────────────────────────────
public class ToggleUserStatusCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public ToggleUserStatusCommandHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var now = DateTimeOffset.UtcNow;
        var isCurrentlyLocked = user.LockoutEnd != null && user.LockoutEnd > now;

        if (isCurrentlyLocked)
        {
            // Desbloquear — quitar el lockout
            await _userManager.SetLockoutEndDateAsync(user, null);
            return Results.Ok(new ToggleUserStatusResponse(user.Id, user.Email!, false, "Activado"));
        }
        else
        {
            // Bloquear — poner lockout indefinido
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return Results.Ok(new ToggleUserStatusResponse(user.Id, user.Email!, true, "Bloqueado"));
        }
    }
}
