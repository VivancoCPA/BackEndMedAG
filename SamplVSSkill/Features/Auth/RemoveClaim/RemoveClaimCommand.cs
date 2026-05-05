using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.RemoveClaim;

// ── Request / Response ──────────────────────────────────────────
// Params via query string: DELETE /api/users/{userId}/claims?claimType=X&claimValue=Y
public record RemoveClaimParams(string ClaimType, string ClaimValue);
public record RemoveClaimResponse(string UserId, string Email, string Message);

// ── Command Handler ─────────────────────────────────────────────
public class RemoveClaimCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public RemoveClaimCommandHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, RemoveClaimParams p, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var existing = await _userManager.GetClaimsAsync(user);
        var claim = existing.FirstOrDefault(c => c.Type == p.ClaimType && c.Value == p.ClaimValue);

        if (claim is null)
            return Results.NotFound($"El claim '{p.ClaimType}:{p.ClaimValue}' no existe para este usuario.");

        var result = await _userManager.RemoveClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        return Results.Ok(new RemoveClaimResponse(user.Id, user.Email!, "Claim eliminado correctamente."));
    }
}
