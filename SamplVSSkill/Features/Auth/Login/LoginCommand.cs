using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Infrastructure.Auth;

namespace SamplVSSkill.Features.Auth.Login;

// ── Request / Response ──────────────────────────────────────────
public record LoginCommand(string Email, string Password);
public record LoginResponse(string Token, string Email);

// ── Validator ───────────────────────────────────────────────────
public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class LoginCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public LoginCommandHandler(UserManager<IdentityUser> userManager, JwtTokenService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<IResult> HandleAsync(LoginCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, command.Password))
        {
            return Results.Problem(
                title: "Credenciales inválidas",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var token = _jwtService.GenerateToken(user);
        return Results.Ok(new LoginResponse(token, user.Email!));
    }
}
