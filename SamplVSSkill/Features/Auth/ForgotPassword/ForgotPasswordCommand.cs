using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Services;

namespace SamplVSSkill.Features.Auth.ForgotPassword;

// ── Request / Response ──────────────────────────────────────────
public record ForgotPasswordCommand(string Email);
public record ForgotPasswordResponse(string Message);

// ── Validator ───────────────────────────────────────────────────
public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class ForgotPasswordCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IResult> HandleAsync(ForgotPasswordCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        const string responseMessage = "Si el correo electrónico ingresado existe en nuestro sistema, recibirás una nueva contraseña temporal por correo.";

        // Always return OK to prevent user enumeration attacks
        if (user is null)
            return Results.Ok(new ForgotPasswordResponse(responseMessage));

        // 1. Autogenerar una contraseña temporal segura
        var tempPassword = GenerateSecureTemporaryPassword();

        // 2. Generar token de reseteo interno y aplicarlo
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );
            return Results.ValidationProblem(errors);
        }

        // 3. Apagar PasswordConfirmed (obligar a cambiarla en su primer login)
        user.PasswordConfirmed = false;
        await _userManager.UpdateAsync(user);

        // 4. Enviar correo con la contraseña temporal autogenerada
        await _emailService.SendForgotPasswordTemporaryPasswordEmailAsync(
            user.Email!,
            $"{user.Name} {user.LastName}",
            tempPassword,
            ct);

        return Results.Ok(new ForgotPasswordResponse(responseMessage));
    }

    private static string GenerateSecureTemporaryPassword()
    {
        var guidUpper = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var guidLower = Guid.NewGuid().ToString("N")[..8].ToLower();
        return $"Temp-{guidUpper}{guidLower}1!";
    }
}

