using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.UpdateFamilyExtraMembership;

// ── Request / Response ──────────────────────────────────────────
public record UpdateFamilyExtraMembershipCommand
{
    public string FullName { get; init; } = default!;
    public string IdType { get; init; } = default!;
    public string? Description { get; init; }
    public IFormFile? Photo { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateFamilyExtraMembershipResponse(
    int Id,
    string FullName,
    string IdType,
    string? PhotoUrl,
    Guid FamilyGroupId,
    string? Description,
    bool IsActive);

// ── Validator ───────────────────────────────────────────────────
public class UpdateFamilyExtraMembershipValidator : AbstractValidator<UpdateFamilyExtraMembershipCommand>
{
    public UpdateFamilyExtraMembershipValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.IdType)
            .NotEmpty().WithMessage("El tipo de documento es requerido.")
            .MaximumLength(50).WithMessage("El tipo de documento no puede exceder 50 caracteres.");
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateFamilyExtraMembershipCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UpdateFamilyExtraMembershipCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(Guid familyGroupId, int id, UpdateFamilyExtraMembershipCommand command, CancellationToken ct)
    {
        // 1. Verificar si el grupo familiar existe
        var groupExists = await _db.FamilyGroups.AnyAsync(g => g.Id == familyGroupId, ct);
        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        // 2. Buscar el miembro extra
        var extraMember = await _db.FamilyExtraMemberships
            .FirstOrDefaultAsync(m => m.Id == id && m.FamilyGroupId == familyGroupId, ct);

        if (extraMember is null)
            return Results.NotFound($"Miembro extra con ID '{id}' no encontrado en el grupo familiar '{familyGroupId}'.");

        // 3. Pre-calcular los detalles de guardado de la nueva foto si existe
        string? oldPhotoUrl = extraMember.PhotoUrl;
        string? newRelativePath = null;
        string? newAbsolutePath = null;

        if (command.Photo is not null)
        {
            var extension = Path.GetExtension(command.Photo.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            
            // Directorio: wwwroot/uploads/extra-members/
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "extra-members");
            
            newRelativePath = $"/uploads/extra-members/{uniqueFileName}";
            newAbsolutePath = Path.Combine(uploadsDir, uniqueFileName);
        }

        // 4. Actualizar datos en DB
        extraMember.FullName = command.FullName;
        extraMember.IdType = command.IdType;
        extraMember.Description = command.Description ?? string.Empty;
        extraMember.IsActive = command.IsActive;

        if (newRelativePath is not null)
        {
            extraMember.PhotoUrl = newRelativePath;
        }

        await _db.SaveChangesAsync(ct);

        // 5. Guardar la nueva foto en disco y limpiar la anterior si es necesario
        if (command.Photo is not null && newAbsolutePath is not null)
        {
            try
            {
                var dir = Path.GetDirectoryName(newAbsolutePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir!);
                }

                using var fileStream = new FileStream(newAbsolutePath, FileMode.Create);
                await command.Photo.CopyToAsync(fileStream, ct);
            }
            catch (Exception ex)
            {
                // Rollback del path de la foto en base de datos
                extraMember.PhotoUrl = oldPhotoUrl;
                await _db.SaveChangesAsync(ct);

                return Results.Problem(
                    title: "Error al actualizar el archivo de foto",
                    detail: $"No se pudo almacenar la nueva foto de perfil en el disco. Error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            // Eliminar la foto anterior si existía una
            if (!string.IsNullOrWhiteSpace(oldPhotoUrl))
            {
                try
                {
                    // Convertir ruta relativa a ruta física
                    var oldPhysicalPath = Path.Combine(_webHostEnvironment.WebRootPath, oldPhotoUrl.TrimStart('/'));
                    if (File.Exists(oldPhysicalPath))
                    {
                        File.Delete(oldPhysicalPath);
                    }
                }
                catch
                {
                    // No bloquear la respuesta si falla la eliminación física de la foto anterior, ya que el flujo principal tuvo éxito
                }
            }
        }

        return Results.Ok(new UpdateFamilyExtraMembershipResponse(
            extraMember.Id,
            extraMember.FullName,
            extraMember.IdType,
            extraMember.PhotoUrl,
            extraMember.FamilyGroupId,
            extraMember.Description,
            extraMember.IsActive));
    }
}
