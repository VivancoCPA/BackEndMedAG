using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.CreateUser;

// ── Request / Response ──────────────────────────────────────────
public record CreateUserCommand
{
    public string Email { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string? Phone { get; init; }
    public string? DateOfBirth { get; init; } // Recibido como string para evitar fallos de model binding por cultura regional
    public IFormFile? Photo { get; init; }
}

public record CreateUserResponse(string Id, string Email, string Name, string LastName, string? PhotoUrl, bool PasswordConfirmed);

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

        RuleFor(x => x.DateOfBirth)
            .Must(dob => string.IsNullOrEmpty(dob) || DateTime.TryParseExact(dob, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
            .WithMessage("La fecha de nacimiento debe estar en formato yyyy-MM-dd.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class CreateUserCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateUserCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;
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

        // Parsear fecha de nacimiento de forma independiente a la cultura del servidor
        DateTime? dateOfBirth = null;
        if (!string.IsNullOrWhiteSpace(command.DateOfBirth) &&
            DateTime.TryParseExact(command.DateOfBirth, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            dateOfBirth = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }

        string? photoUrl = null;
        string? extension = null;
        string? uniqueFileName = null;

        // 1. Pre-calcular el nombre de archivo y la ruta de la foto si existe
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            photoUrl = $"/uploads/profiles/{uniqueFileName}";
        }

        var user = new AppUser
        {
            UserName = command.Email,
            Email = command.Email,
            Name = command.Name,
            LastName = command.LastName,
            PhoneNumber = command.Phone,
            DateOfBirth = dateOfBirth,
            PhotoUrl = photoUrl,
            EmailConfirmed = true, // Confirmado automáticamente por el administrador
            PasswordConfirmed = false // Debe cambiarla al ingresar
        };

        // 2. Intentar guardar el registro del usuario en la base de datos PRIMERO
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

        // 3. Físicamente guardar el archivo localmente SOLAMENTE si la base de datos tuvo éxito
        if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
        {
            try
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "profiles");

                // Crear carpeta si no existe
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.Photo.CopyToAsync(stream, ct);
                }
            }
            catch (Exception)
            {
                // Si la escritura en el disco local falla físicamente, hacemos un ROLLBACK eliminando el registro del usuario creado
                await _userManager.DeleteAsync(user);
                
                return Results.Problem(
                    title: "Error al almacenar la foto de perfil",
                    detail: "El usuario no pudo ser creado debido a un problema físico al intentar guardar su imagen en el servidor.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        // Enviar correo de bienvenida con la contraseña temporal
        await _emailService.SendTemporaryPasswordEmailAsync(user.Email!, user.Name, tempPassword, ct);

        return Results.Created(
            $"/api/auth/users",
            new CreateUserResponse(user.Id, user.Email!, user.Name, user.LastName, user.PhotoUrl, user.PasswordConfirmed));
    }

    private static string GenerateSecureTemporaryPassword()
    {
        var guidUpper = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var guidLower = Guid.NewGuid().ToString("N")[..8].ToLower();
        return $"Temp-{guidUpper}{guidLower}1!";
    }
}


