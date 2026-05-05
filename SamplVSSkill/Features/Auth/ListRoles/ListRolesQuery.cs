using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.ListRoles;

// ── Response ────────────────────────────────────────────────────
public record ListRolesResponse(string Id, string Name);

// ── Query Handler ───────────────────────────────────────────────
public class ListRolesQueryHandler
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public ListRolesQueryHandler(RoleManager<IdentityRole> roleManager) =>
        _roleManager = roleManager;

    public Task<IEnumerable<ListRolesResponse>> HandleAsync(CancellationToken ct)
    {
        var roles = _roleManager.Roles
            .Select(r => new ListRolesResponse(r.Id, r.Name!))
            .AsEnumerable();

        return Task.FromResult(roles);
    }
}
