using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Insurers.CreateInsurer;

// ── Request / Response ──────────────────────────────────────────
public record CreateInsurerCommand
{
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string? PersonInCharge { get; init; }
    public IFormFile? Photo { get; init; }
}

public record CreateInsurerResponse(
    Guid Id, string Name, string Address, string Phone, string Email,
    string? PersonInCharge, string? LogoUrl, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

// ── Validator ───────────────────────────────────────────────────
public class CreateInsurerValidator : AbstractValidator<CreateInsurerCommand>
{
    public CreateInsurerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class CreateInsurerCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateInsurerCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(
        CreateInsurerCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        string? logoUrl = null;
        string? extension = null;
        string? uniqueFileName = null;

        // 1. Pre-calcular el nombre de archivo y la ruta del logo si existe
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            logoUrl = $"/uploads/insurers/{uniqueFileName}";
        }

        var insurer = new Insurer
        {
            Id            = Guid.CreateVersion7(),
            Name          = command.Name,
            Address       = command.Address,
            Phone         = command.Phone,
            Email         = command.Email,
            PersonInCharge = command.PersonInCharge,
            LogoUrl       = logoUrl,
            IsActive      = true,
            CreatedAt     = now,
            UpdatedAt     = now
        };

        // 2. Intentar guardar el registro en la base de datos PRIMERO
        _db.Insurers.Add(insurer);
        await _db.SaveChangesAsync(ct);

        // 3. Físicamente guardar el archivo localmente SOLAMENTE si la base de datos tuvo éxito
        if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
        {
            try
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "insurers");

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
                // Si la escritura en el disco local falla físicamente, hacemos un ROLLBACK eliminando la aseguradora creada
                _db.Insurers.Remove(insurer);
                await _db.SaveChangesAsync(ct);
                
                return Results.Problem(
                    title: "Error al almacenar el logo de la aseguradora",
                    detail: "La aseguradora no pudo ser creada debido a un problema físico al intentar guardar su logo en el servidor.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Created(
            $"/api/insurers/{insurer.Id}",
            new CreateInsurerResponse(
                insurer.Id, insurer.Name, insurer.Address, insurer.Phone, insurer.Email,
                insurer.PersonInCharge, insurer.LogoUrl, insurer.IsActive,
                insurer.CreatedAt, insurer.UpdatedAt));
    }
}
