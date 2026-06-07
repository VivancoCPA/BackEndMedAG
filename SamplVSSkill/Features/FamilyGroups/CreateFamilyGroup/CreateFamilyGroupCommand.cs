using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyGroups.CreateFamilyGroup;

// ── Request / Response ──────────────────────────────────────────
public record CreateFamilyGroupCommand
{
    public string Name { get; init; } = default!;
    public string? UserId { get; init; }
    public IFormFile? Photo { get; init; }
}

public record CreateFamilyGroupResponse(Guid Id, string Name, string? UserId, bool IsActive, DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class CreateFamilyGroupValidator : AbstractValidator<CreateFamilyGroupCommand>
{
    public CreateFamilyGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class CreateFamilyGroupCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateFamilyGroupCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(CreateFamilyGroupCommand command, CancellationToken ct)
    {
        // Pre-calcular el path de la foto si existe
        string? relativePath = null;
        string? absolutePath = null;

        if (command.Photo is not null && command.Photo.Length > 0)
        {
            var extension = Path.GetExtension(command.Photo.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            
            // Directorio: wwwroot/uploads/family/
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "family");
            
            relativePath = $"/uploads/family/{uniqueFileName}";
            absolutePath = Path.Combine(uploadsDir, uniqueFileName);
        }

        var group = new FamilyGroup
        {
            Id        = Guid.NewGuid(),
            Name      = command.Name,
            UserId    = command.UserId,
            PhotoUrl  = relativePath,
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.FamilyGroups.Add(group);
        await _db.SaveChangesAsync(ct);

        // Guardar archivo físico
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
                _db.FamilyGroups.Remove(group);
                await _db.SaveChangesAsync(ct);

                return Results.Problem(
                    title: "Error al guardar el archivo de foto",
                    detail: $"No se pudo almacenar la foto del grupo en el disco. Error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Created($"/api/family-groups/{group.Id}",
            new CreateFamilyGroupResponse(group.Id, group.Name, group.UserId, group.IsActive, group.CreatedAt));
    }
}
