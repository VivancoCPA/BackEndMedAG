using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Auth;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.RefreshToken;

// ── Request / Response ──────────────────────────────────────────
public record RefreshTokenCommand(string Token, string RefreshToken);
public record RefreshTokenResponse(string Token, string RefreshToken);

// ── Validator ───────────────────────────────────────────────────
public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El Access Token es requerido.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El Refresh Token es requerido.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class RefreshTokenCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public RefreshTokenCommandHandler(UserManager<AppUser> userManager, JwtTokenService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<IResult> HandleAsync(RefreshTokenCommand command, CancellationToken ct)
    {
        ClaimsPrincipal? principal;
        try
        {
            principal = _jwtService.GetPrincipalFromExpiredToken(command.Token);
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Token de acceso inválido",
                detail: "El Access Token proporcionado es inválido o no pudo ser procesado.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (principal is null)
        {
            return Results.Problem(
                title: "Token de acceso inválido",
                detail: "No se pudieron obtener los claims del token.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Obtener el ID del usuario (soporta ClaimTypes.NameIdentifier y claim estándar "sub")
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                title: "Token de acceso inválido",
                detail: "El token no contiene un identificador de usuario válido.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshToken != command.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Results.Problem(
                title: "Token de refresco inválido o expirado",
                detail: "El Refresh Token proporcionado no coincide, es inválido o ha expirado.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Generar nuevos tokens (Rotación)
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        return Results.Ok(new RefreshTokenResponse(newAccessToken, newRefreshToken));
    }
}
