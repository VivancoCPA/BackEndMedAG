using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.AssignClaim;

// ── Request / Response ──────────────────────────────────────────
public record AssignClaimCommand(string ClaimType, string ClaimValue);
public record AssignClaimResponse(string UserId, string Email, string ClaimType, string ClaimValue);

// ── Validator ───────────────────────────────────────────────────
public class AssignClaimValidator : AbstractValidator<AssignClaimCommand>
{
    public AssignClaimValidator()
    {
        RuleFor(x => x.ClaimType)
            .NotEmpty().WithMessage("El tipo de claim es requerido.")
            .MaximumLength(100).WithMessage("El tipo de claim no puede exceder 100 caracteres.");

        RuleFor(x => x.ClaimValue)
            .NotEmpty().WithMessage("El valor del claim es requerido.")
            .MaximumLength(256).WithMessage("El valor del claim no puede exceder 256 caracteres.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class AssignClaimCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public AssignClaimCommandHandler(UserManager<IdentityUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, AssignClaimCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        var existing = await _userManager.GetClaimsAsync(user);
        if (existing.Any(c => c.Type == command.ClaimType && c.Value == command.ClaimValue))
            return Results.Conflict($"El claim '{command.ClaimType}:{command.ClaimValue}' ya existe.");

        var result = await _userManager.AddClaimAsync(user, new Claim(command.ClaimType, command.ClaimValue));

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        return Results.Created($"/api/users/{userId}/claims",
            new AssignClaimResponse(user.Id, user.Email!, command.ClaimType, command.ClaimValue));
    }
}
