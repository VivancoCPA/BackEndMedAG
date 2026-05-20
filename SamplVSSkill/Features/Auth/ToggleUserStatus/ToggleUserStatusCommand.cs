using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

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
    private readonly UserManager<AppUser> _userManager;

    public ToggleUserStatusCommandHandler(UserManager<AppUser> userManager) =>
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
            // Desbloquear: limpiar LockoutEnd. LockoutEnabled se mantiene en TRUE (comportamiento correcto de Identity).
            user.LockoutEnd = null;
        }
        else
        {
            // Bloquear: poner LockoutEnd en el futuro lejano.
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        var locked = user.LockoutEnd != null && user.LockoutEnd > now;
        return Results.Ok(new ToggleUserStatusResponse(user.Id, user.Email!, locked, locked ? "Bloqueado" : "Activado"));
    }
}
