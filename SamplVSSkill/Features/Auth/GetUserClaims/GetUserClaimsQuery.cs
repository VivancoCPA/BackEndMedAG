using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.GetUserClaims;

// ── Response ────────────────────────────────────────────────────
public record ClaimItem(string Type, string Value);
public record GetUserClaimsResponse(string UserId, string Email, IList<ClaimItem> Claims);

// ── Query Handler ───────────────────────────────────────────────
public class GetUserClaimsQueryHandler
{
    private readonly UserManager<AppUser> _userManager;

    public GetUserClaimsQueryHandler(UserManager<AppUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var claims = await _userManager.GetClaimsAsync(user);

        var claimItems = claims
            .Select(c => new ClaimItem(c.Type, c.Value))
            .ToList();

        return Results.Ok(new GetUserClaimsResponse(user.Id, user.Email!, claimItems));
    }
}
