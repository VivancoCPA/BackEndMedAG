using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Auth;

namespace SamplVSSkill.Features.Auth.Login;

// ── Request / Response ──────────────────────────────────────────
public record LoginCommand(string Email, string Password);
public record LoginResponse(string Token, string RefreshToken, string Email, string Name, string LastName, bool PasswordConfirmed);

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
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public LoginCommandHandler(UserManager<AppUser> userManager, JwtTokenService jwtService)
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

        // Verificar si el usuario está bloqueado (solo LockoutEnd determina el bloqueo activo)
        if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return Results.Problem(
                title: "Usuario bloqueado",
                detail: "Tu cuenta se encuentra bloqueada. Contacta al administrador.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpirationDays);
        user.LastAccess = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Results.Ok(new LoginResponse(token, refreshToken, user.Email!, user.Name, user.LastName, user.PasswordConfirmed));
    }
}

