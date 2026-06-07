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

namespace SamplVSSkill.Features.FamilyExtraMemberships.CreateFamilyExtraMembership;

// ── Request / Response ──────────────────────────────────────────
public record CreateFamilyExtraMembershipCommand
{
    public string FullName { get; init; } = default!;
    public string IdType { get; init; } = default!;
    public string? Description { get; init; }
    public IFormFile? Photo { get; init; }
}

public record CreateFamilyExtraMembershipResponse(
    int Id,
    string FullName,
    string IdType,
    string? PhotoUrl,
    Guid FamilyGroupId,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class CreateFamilyExtraMembershipValidator : AbstractValidator<CreateFamilyExtraMembershipCommand>
{
    public CreateFamilyExtraMembershipValidator()
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
public class CreateFamilyExtraMembershipCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateFamilyExtraMembershipCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(Guid familyGroupId, CreateFamilyExtraMembershipCommand command, CancellationToken ct)
    {
        // 1. Verificar si el grupo familiar existe
        var groupExists = await _db.FamilyGroups.AnyAsync(g => g.Id == familyGroupId, ct);
        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        // 2. Pre-calcular los detalles de guardado de la foto si existe
        string? relativePath = null;
        string? absolutePath = null;

        if (command.Photo is not null)
        {
            var extension = Path.GetExtension(command.Photo.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            
            // Directorio: wwwroot/uploads/extra-members/
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "extra-members");
            
            relativePath = $"/uploads/extra-members/{uniqueFileName}";
            absolutePath = Path.Combine(uploadsDir, uniqueFileName);
        }

        // 3. Crear registro del miembro extra
        var extraMember = new FamilyExtraMembership
        {
            FullName = command.FullName,
            IdType = command.IdType,
            Description = command.Description ?? string.Empty,
            FamilyGroupId = familyGroupId,
            PhotoUrl = relativePath ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.FamilyExtraMemberships.Add(extraMember);
        await _db.SaveChangesAsync(ct);

        // 4. Guardar archivo físico
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
            }
            catch (Exception ex)
            {
                // Rollback de base de datos
                _db.FamilyExtraMemberships.Remove(extraMember);
                await _db.SaveChangesAsync(ct);

                return Results.Problem(
                    title: "Error al guardar el archivo de foto",
                    detail: $"No se pudo almacenar la foto de perfil en el disco. Error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Created(
            $"/api/family-groups/{familyGroupId}/extra-members/{extraMember.Id}",
            new CreateFamilyExtraMembershipResponse(
                extraMember.Id,
                extraMember.FullName,
                extraMember.IdType,
                extraMember.PhotoUrl,
                extraMember.FamilyGroupId,
                extraMember.Description,
                extraMember.IsActive,
                extraMember.CreatedAt));
    }
}
