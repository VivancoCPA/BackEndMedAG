using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyGroups.UpdateFamilyGroup;

// ── Request / Response ──────────────────────────────────────────
public record UpdateFamilyGroupCommand
{
    public string Name { get; init; } = default!;
    public string? UserId { get; init; }
    public IFormFile? Photo { get; init; }
}

public record UpdateFamilyGroupResponse(Guid Id, string Name, string? UserId, string? PhotoUrl, bool IsActive);

// ── Validator ───────────────────────────────────────────────────
public class UpdateFamilyGroupValidator : AbstractValidator<UpdateFamilyGroupCommand>
{
    public UpdateFamilyGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateFamilyGroupCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UpdateFamilyGroupCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(Guid id, UpdateFamilyGroupCommand command, CancellationToken ct)
    {
        var group = await _db.FamilyGroups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return Results.NotFound($"Grupo familiar '{id}' no encontrado.");

        var oldPhotoUrl = group.PhotoUrl;
        var photoUrl = group.PhotoUrl;
        string? uniqueFileName = null;
        string? absolutePath = null;

        // 1. Pre-calcular el nuevo path si se proporciona una nueva foto
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            var extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "family");
            
            photoUrl = $"/uploads/family/{uniqueFileName}";
            absolutePath = Path.Combine(uploadsDir, uniqueFileName);
        }

        group.Name   = command.Name;
        group.UserId = command.UserId;
        group.PhotoUrl = photoUrl;

        // 2. Guardar en base de datos primero
        await _db.SaveChangesAsync(ct);

        // 3. Guardar archivo físico en disco
        if (command.Photo is not null && absolutePath is not null)
        {
            try
            {
                var dir = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir!);
                }

                using var fileStream = new FileStream(absolutePath, FileMode.Create);
                await command.Photo.CopyToAsync(fileStream, ct);

                // Si la copia física tuvo éxito, limpiar el archivo anterior si existía
                if (!string.IsNullOrEmpty(oldPhotoUrl))
                {
                    try
                    {
                        var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        // Remover el '/' inicial de la URL relativa para construir el path físico correcto
                        var cleanOldUrl = oldPhotoUrl.TrimStart('/');
                        var oldFilePath = Path.Combine(webRootPath, cleanOldUrl.Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }
                    catch
                    {
                        // Ignorar errores al eliminar la foto vieja
                    }
                }
            }
            catch (Exception ex)
            {
                // Rollback: revertir el PhotoUrl anterior en base de datos
                group.PhotoUrl = oldPhotoUrl;
                await _db.SaveChangesAsync(ct);

                return Results.Problem(
                    title: "Error al almacenar la nueva foto del grupo familiar",
                    detail: $"No se pudo guardar la foto en el disco. Error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Ok(new UpdateFamilyGroupResponse(group.Id, group.Name, group.UserId, group.PhotoUrl, group.IsActive));
    }
}
