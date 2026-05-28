using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Auth;

namespace SamplVSSkill.Features.Auth.Register;

// ── Request / Response ──────────────────────────────────────────
public record RegisterCommand(
    string Name,
    string LastName,
    string Email,
    string Password,
    string Phone,
    DateTime? DateOfBirth);
public record RegisterResponse(string Token, string RefreshToken, string Email, string Name, string LastName);

// ── Validator ───────────────────────────────────────────────────
public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe tener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe tener al menos una letra minúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe tener al menos un dígito.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe tener al menos un caracter especial (* @ ! etc).");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class RegisterCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public RegisterCommandHandler(UserManager<AppUser> userManager, JwtTokenService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<IResult> HandleAsync(RegisterCommand command, CancellationToken ct)
    {
        var user = new AppUser
        {
            UserName    = command.Email,
            Email       = command.Email,
            Name        = command.Name,
            LastName    = command.LastName,
            PhoneNumber = command.Phone,
            DateOfBirth = command.DateOfBirth,
            PasswordConfirmed = true
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
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Results.Created("/api/auth/register", new RegisterResponse(token, refreshToken, user.Email!, user.Name, user.LastName));
    }
}
