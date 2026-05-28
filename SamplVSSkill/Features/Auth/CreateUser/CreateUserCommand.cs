using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Services;
using System.Security.Cryptography;

namespace SamplVSSkill.Features.Auth.CreateUser;

// ── Request / Response ──────────────────────────────────────────
public record CreateUserCommand(string Email, string Name, string LastName, string? Phone, DateTime? DateOfBirth);
public record CreateUserResponse(string Id, string Email, string Name, string LastName, bool PasswordConfirmed);

// ── Validator ───────────────────────────────────────────────────
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100);
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class CreateUserCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;

    public CreateUserCommandHandler(UserManager<AppUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IResult> HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
        {
            return Results.Conflict($"El email '{command.Email}' ya se encuentra registrado.");
        }

        // Autogenerar una contraseña temporal segura
        // Debe cumplir con: mín. 8 caracteres, 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial
        var tempPassword = GenerateSecureTemporaryPassword();

        var user = new AppUser
        {
            UserName = command.Email,
            Email = command.Email,
            Name = command.Name,
            LastName = command.LastName,
            PhoneNumber = command.Phone,
            DateOfBirth = command.DateOfBirth,
            EmailConfirmed = true, // Confirmado automáticamente por el administrador
            PasswordConfirmed = false // Debe cambiarla al ingresar
        };

        var result = await _userManager.CreateAsync(user, tempPassword);

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

        // Enviar correo de bienvenida con la contraseña temporal
        await _emailService.SendTemporaryPasswordEmailAsync(user.Email!, user.Name, tempPassword, ct);

        return Results.Created(
            $"/api/auth/users",
            new CreateUserResponse(user.Id, user.Email!, user.Name, user.LastName, user.PasswordConfirmed));
    }

    private static string GenerateSecureTemporaryPassword()
    {
        var guidUpper = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var guidLower = Guid.NewGuid().ToString("N")[..8].ToLower();
        return $"Temp-{guidUpper}{guidLower}1!";
    }
}
