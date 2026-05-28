using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.ResetPassword;

// ── Request / Response ──────────────────────────────────────────
public record ResetPasswordCommand(string Email, string Token, string NewPassword);
public record ResetPasswordResponse(string Email, string Message);

// ── Validator ───────────────────────────────────────────────────
public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty().WithMessage("El token de recuperación es requerido.");
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe tener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe tener al menos una letra minúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe tener al menos un dígito.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe tener al menos un caracter especial (* @ ! etc).");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class ResetPasswordCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<IResult> HandleAsync(ResetPasswordCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Results.Problem(
                title: "Token inválido o expirado",
                statusCode: StatusCodes.Status400BadRequest);

        // URL-decode the token in case it arrives encoded from the frontend link
        var decodedToken = Uri.UnescapeDataString(command.Token);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, command.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        user.PasswordConfirmed = true;
        await _userManager.UpdateAsync(user);

        return Results.Ok(new ResetPasswordResponse(user.Email!, "Contraseña restablecida exitosamente."));
    }
}
