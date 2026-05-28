using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.ChangePassword;

// ── Request / Response ──────────────────────────────────────────
public record ChangePasswordCommand(string Email, string CurrentPassword, string NewPassword);
public record ChangePasswordResponse(string Email, string Message);

// ── Validator ───────────────────────────────────────────────────
public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe tener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe tener al menos una letra minúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe tener al menos un dígito.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe tener al menos un caracter especial (* @ ! etc).")
            .NotEqual(x => x.CurrentPassword).WithMessage("La nueva contraseña debe ser diferente a la actual.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class ChangePasswordCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<IResult> HandleAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Results.NotFound($"Usuario con email '{command.Email}' no encontrado.");

        var result = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        user.PasswordConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Results.Ok(new ChangePasswordResponse(user.Email!, "Contraseña cambiada exitosamente."));
    }
}
