using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
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
    public string? Address { get; init; } = default!;
    public string? Phone { get; init; }
    public string? DateOfBirth { get; init; } // Recibido como string para evitar fallos de model binding por cultura regional
    public IFormFile? Photo { get; init; }
}

public record CreateUserResponse(string Id, string Email, string Name, string LastName, string? Address, string? PhotoUrl, bool PasswordConfirmed);

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
    private readonly AppDbContext _db;

    public CreateUserCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IWebHostEnvironment webHostEnvironment,
        AppDbContext db)
    {
        _userManager = userManager;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;
        _db = db;
    }

    public async Task<IResult> HandleAsync(CreateUserCommand command, string creatorUserId, bool isCreatorAdmin, CancellationToken ct)
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
            Address = command.Address,
            PhoneNumber = command.Phone,
            DateOfBirth = dateOfBirth,
            PhotoUrl = photoUrl,
            EmailConfirmed = true, // Confirmado automáticamente por el administrador
            PasswordConfirmed = false // Debe cambiarla al ingresar
        };

        // Iniciar transacción de base de datos para asegurar atomicidad
        using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 2. Intentar guardar el registro del usuario en la base de datos
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

            if (isCreatorAdmin)
            {
                // Crear asociación de UserScope para asociar el nuevo usuario al administrador creador
                var userScope = new UserScope
                {
                    UserIdAdmin = creatorUserId,
                    User = user
                };
                _db.UserScopes.Add(userScope);
                await _db.SaveChangesAsync(ct);
            }

            // 3. Físicamente guardar el archivo localmente SOLAMENTE si la base de datos tuvo éxito
            if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
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

            // Si todo tiene éxito, confirmamos la transacción
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            // La transacción hará rollback automáticamente al disponerse si no se llamó a CommitAsync.
            // Pero llamamos a RollbackAsync explícitamente para mayor claridad.
            await transaction.RollbackAsync(ct);

            // Si la foto fue guardada físicamente antes de que fallara la base de datos, la eliminamos
            if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
            {
                try
                {
                    var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var filePath = Path.Combine(webRootPath, "uploads", "profiles", uniqueFileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch
                {
                    // Ignorar errores al intentar borrar la foto huérfana
                }
            }

            return Results.Problem(
                title: "Error al crear el usuario",
                detail: "El usuario no pudo ser creado debido a un problema al registrar sus datos.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        // Enviar correo de bienvenida con la contraseña temporal
        await _emailService.SendTemporaryPasswordEmailAsync(user.Email!, user.Name, tempPassword, ct);

        return Results.Created(
            $"/api/auth/users",
            new CreateUserResponse(user.Id, user.Email!, user.Name, user.LastName, user.Address, user.PhotoUrl, user.PasswordConfirmed));
    }

    private static string GenerateSecureTemporaryPassword()
    {
        var guidUpper = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var guidLower = Guid.NewGuid().ToString("N")[..8].ToLower();
        return $"Temp-{guidUpper}{guidLower}1!";
    }
}


