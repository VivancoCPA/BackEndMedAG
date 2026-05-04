using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Infrastructure.Auth;

namespace SamplVSSkill.Features.Auth.Register;

// ── Request / Response ──────────────────────────────────────────
public record RegisterCommand(string Email, string Password);
public record RegisterResponse(string Token, string Email);

// ── Validator ───────────────────────────────────────────────────
public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class RegisterCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public RegisterCommandHandler(UserManager<IdentityUser> userManager, JwtTokenService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<IResult> HandleAsync(RegisterCommand command, CancellationToken ct)
    {
        var user = new IdentityUser
        {
            UserName = command.Email,
            Email = command.Email
        };

        var result = await _userManager.CreateAsync(user, command.Password);

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

        var token = _jwtService.GenerateToken(user);
        return Results.Created("/api/auth/register", new RegisterResponse(token, user.Email!));
    }
}
