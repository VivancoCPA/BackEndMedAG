using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.CreateRole;

// ── Request / Response ──────────────────────────────────────────
public record CreateRoleCommand(string RoleName, string? Description = null, bool? IsActive = true);
public record CreateRoleResponse(string Id, string Name, string Description, bool IsActive, DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("El nombre del rol es requerido.")
            .MaximumLength(50).WithMessage("El nombre del rol no puede exceder 50 caracteres.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class CreateRoleCommandHandler
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager) =>
        _roleManager = roleManager;

    public async Task<IResult> HandleAsync(CreateRoleCommand command, CancellationToken ct)
    {
        if (await _roleManager.RoleExistsAsync(command.RoleName))
            return Results.Conflict($"El rol '{command.RoleName}' ya existe.");

        var result = await _roleManager.CreateAsync(new ApplicationRole 
        { 
            Name = command.RoleName,
            Description = command.Description ?? string.Empty,
            IsActive = command.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        });

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        var role = await _roleManager.FindByNameAsync(command.RoleName);
        return Results.Created("/api/roles", new CreateRoleResponse(role!.Id, role.Name!, role.Description, role.IsActive, role.CreatedAt));
    }
}
