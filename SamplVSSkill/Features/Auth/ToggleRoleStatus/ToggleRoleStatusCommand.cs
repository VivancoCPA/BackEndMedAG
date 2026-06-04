using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.ToggleRoleStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleRoleStatusResponse(string Id, string Name, bool IsActive, string Status);

// ── Command Handler ─────────────────────────────────────────────
public class ToggleRoleStatusCommandHandler
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ToggleRoleStatusCommandHandler(RoleManager<ApplicationRole> roleManager) =>
        _roleManager = roleManager;

    public async Task<IResult> HandleAsync(string id, CancellationToken ct)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return Results.NotFound($"El rol con ID '{id}' no existe.");

        role.IsActive = !role.IsActive;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        var status = role.IsActive ? "Activado" : "Inactivado";
        return Results.Ok(new ToggleRoleStatusResponse(
            role.Id,
            role.Name!,
            role.IsActive,
            status));
    }
}
