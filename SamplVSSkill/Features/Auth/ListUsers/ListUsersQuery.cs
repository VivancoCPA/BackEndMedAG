using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.ListUsers;

// ── Response ────────────────────────────────────────────────────
public record ListUsersResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    DateTime? DateOfBirth,
    Guid? InsurerId,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd);

// ── Query Handler ───────────────────────────────────────────────
public class ListUsersQueryHandler
{
    private readonly UserManager<AppUser> _userManager;

    public ListUsersQueryHandler(UserManager<AppUser> userManager) =>
        _userManager = userManager;

    public Task<IEnumerable<ListUsersResponse>> HandleAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var users = _userManager.Users
            .Select(u => new ListUsersResponse(
                u.Id,
                u.Email!,
                u.Name,
                u.LastName,
                u.DateOfBirth,
                u.InsurerId,
                u.EmailConfirmed,
                u.LockoutEnd != null && u.LockoutEnd > now,
                u.LockoutEnd))
            .AsEnumerable();

        return Task.FromResult(users);
    }
}
