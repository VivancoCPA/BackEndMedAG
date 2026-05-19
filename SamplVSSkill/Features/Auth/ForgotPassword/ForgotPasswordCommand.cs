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
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<IResult> HandleAsync(ForgotPasswordCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        // Always return OK to prevent user enumeration attacks
        if (user is null)
            return Results.Ok(new ForgotPasswordResponse(
                "Si el correo existe en nuestro sistema, recibirás las instrucciones de recuperación."));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // URL-encode the token (it can contain special chars)
        var encodedToken = Uri.EscapeDataString(token);
        var frontendBaseUrl = _configuration["App:FrontendUrl"] ?? "https://localhost:3000";
        var resetLink = $"{frontendBaseUrl}/auth/reset-password?email={command.Email}&token={encodedToken}";

        await _emailService.SendPasswordResetEmailAsync(
            user.Email!,
            $"{user.Name} {user.LastName}",
            resetLink,
            ct);

        return Results.Ok(new ForgotPasswordResponse(
            "Si el correo existe en nuestro sistema, recibirás las instrucciones de recuperación."));
    }
}
