using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.UpdateRole;

// ── Request / Response ──────────────────────────────────────────
public record UpdateRoleCommand(string RoleName, string Description, bool IsActive);
public record UpdateRoleResponse(string Id, string Name, string Description, bool IsActive, DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("El nombre del rol es requerido.")
            .MaximumLength(50).WithMessage("El nombre del rol no puede exceder 50 caracteres.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class UpdateRoleCommandHandler
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UpdateRoleCommandHandler(RoleManager<ApplicationRole> roleManager) =>
        _roleManager = roleManager;

    public async Task<IResult> HandleAsync(string id, UpdateRoleCommand command, CancellationToken ct)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return Results.NotFound($"El rol con ID '{id}' no existe.");

        // If the role name changes, check for unique constraint
        if (!string.Equals(role.Name, command.RoleName, StringComparison.OrdinalIgnoreCase))
        {
            if (await _roleManager.RoleExistsAsync(command.RoleName))
                return Results.Conflict($"El rol '{command.RoleName}' ya existe.");
            
            role.Name = command.RoleName;
        }

        role.Description = command.Description ?? string.Empty;
        role.IsActive = command.IsActive;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        return Results.Ok(new UpdateRoleResponse(
            role.Id, 
            role.Name!, 
            role.Description, 
            role.IsActive, 
            role.CreatedAt));
    }
}
