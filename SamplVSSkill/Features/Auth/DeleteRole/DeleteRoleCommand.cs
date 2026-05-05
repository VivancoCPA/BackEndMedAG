using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.DeleteRole;

// ── Response ────────────────────────────────────────────────────
public record DeleteRoleResponse(string RoleName, string Message);

// ── Command Handler ─────────────────────────────────────────────
public class DeleteRoleCommandHandler
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public DeleteRoleCommandHandler(RoleManager<IdentityRole> roleManager) =>
        _roleManager = roleManager;

    public async Task<IResult> HandleAsync(string roleName, CancellationToken ct)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role is null)
            return Results.NotFound($"El rol '{roleName}' no existe.");

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
        {
            // Identity returns errors if the role has users assigned
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        return Results.Ok(new DeleteRoleResponse(roleName, "Rol eliminado correctamente."));
    }
}
