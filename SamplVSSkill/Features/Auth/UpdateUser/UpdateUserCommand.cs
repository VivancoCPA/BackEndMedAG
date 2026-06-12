using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.UpdateUser;

// ── Request / Response ──────────────────────────────────────────
public record UpdateUserCommand
{
    public string Name { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string? DateOfBirth { get; init; } // Recibido como string para evitar inconsistencias de formato por cultura/región
    public string? PhoneNumber { get; init; }
    public IFormFile? Photo { get; init; }
    public string? Address { get; init; }
}

public record UpdateUserResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    DateTime? DateOfBirth,
    string? PhoneNumber,
    string? PhotoUrl,
    string? Address);

// ── Validator ───────────────────────────────────────────────────
public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => string.IsNullOrEmpty(dob) || DateTime.TryParseExact(dob, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
            .WithMessage("La fecha de nacimiento debe estar en formato yyyy-MM-dd.");
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class UpdateUserCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly DapperConnectionFactory _connectionFactory;

    public UpdateUserCommandHandler(
        UserManager<AppUser> userManager,
        IWebHostEnvironment webHostEnvironment,
        DapperConnectionFactory connectionFactory)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
        _connectionFactory = connectionFactory;
    }

    public async Task<IResult> HandleAsync(
        string userId, UpdateUserCommand command, string currentUserId, bool bypassScope, CancellationToken ct)
    {
        if (!bypassScope && userId != currentUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var inScope = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    "SELECT EXISTS(SELECT 1 FROM user_scope WHERE user_id_admin = @CurrentUserId AND user_id = @UserId)",
                    new { CurrentUserId = currentUserId, UserId = userId },
                    cancellationToken: ct));

            if (!inScope)
            {
                return Results.Forbid();
            }
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        // Parsear fecha de nacimiento de forma independiente a la cultura del servidor
        DateTime? dateOfBirth = null;
        if (!string.IsNullOrWhiteSpace(command.DateOfBirth) &&
            DateTime.TryParseExact(command.DateOfBirth, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            dateOfBirth = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }

        var oldPhotoUrl = user.PhotoUrl;
        var photoUrl = user.PhotoUrl;
        string? uniqueFileName = null;
        string? extension = null;

        // 1. Pre-calcular la nueva foto si se proporciona una
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            photoUrl = $"/uploads/profiles/{uniqueFileName}";
        }

        user.Name        = command.Name;
        user.LastName    = command.LastName;
        user.DateOfBirth = dateOfBirth;
        user.PhoneNumber = command.PhoneNumber;
        user.PhotoUrl    = photoUrl;
        user.Address     = command.Address;

        // 2. Guardar en base de datos primero
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        // 3. Físicamente guardar el archivo localmente si la base de datos tuvo éxito
        if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
        {
            try
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "profiles");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.Photo.CopyToAsync(stream, ct);
                }

                // Borrar el archivo viejo si existe
                if (!string.IsNullOrWhiteSpace(oldPhotoUrl))
                {
                    var oldFilePath = Path.Combine(webRootPath, oldPhotoUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        try
                        {
                            File.Delete(oldFilePath);
                        }
                        catch
                        {
                            // Ignorar error al borrar el viejo, no es crítico
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Rollback: revertir el path de foto y guardar de nuevo
                user.PhotoUrl = oldPhotoUrl;
                await _userManager.UpdateAsync(user);

                return Results.Problem(
                    title: "Error al almacenar la nueva foto de perfil",
                    detail: "El perfil no pudo ser actualizado debido a un problema físico al intentar guardar su imagen en el servidor.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Ok(new UpdateUserResponse(
            user.Id, user.Email!, user.Name, user.LastName,
            user.DateOfBirth, user.PhoneNumber, user.PhotoUrl, user.Address));
    }
}

