using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.ListUsers;

// ── Response ────────────────────────────────────────────────────
public record ListUsersResponse(
    string Id,
    string Email,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd);

// ── Query Handler ───────────────────────────────────────────────
public class ListUsersQueryHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public ListUsersQueryHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public Task<IEnumerable<ListUsersResponse>> HandleAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var users = _userManager.Users
            .Select(u => new ListUsersResponse(
                u.Id,
                u.Email!,
                u.EmailConfirmed,
                u.LockoutEnd != null && u.LockoutEnd > now,
                u.LockoutEnd))
            .AsEnumerable();

        return Task.FromResult(users);
    }
}
