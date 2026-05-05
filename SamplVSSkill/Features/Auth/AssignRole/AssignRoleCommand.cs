using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Features.Auth.AssignRole;

// ── Request / Response ──────────────────────────────────────────
public record AssignRoleCommand(string RoleName);
public record AssignRoleResponse(string UserId, string Email, IList<string> Roles);

// ── Validator ───────────────────────────────────────────────────
public class AssignRoleValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("El nombre del rol es requerido.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class AssignRoleCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public AssignRoleCommandHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, AssignRoleCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        if (await _userManager.IsInRoleAsync(user, command.RoleName))
            return Results.Conflict($"El usuario ya tiene el rol '{command.RoleName}'.");

        var result = await _userManager.AddToRoleAsync(user, command.RoleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Results.Ok(new AssignRoleResponse(user.Id, user.Email!, roles));
    }
}
