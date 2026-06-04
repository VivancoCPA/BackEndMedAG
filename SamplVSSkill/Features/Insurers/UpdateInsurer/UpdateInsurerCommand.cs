using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Insurers.UpdateInsurer;

// ── Request / Response ──────────────────────────────────────────
public record UpdateInsurerCommand
{
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string? PersonInCharge { get; init; }
    public IFormFile? Photo { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateInsurerResponse(
    Guid Id, string Name, string Address, string Phone, string Email,
    string? PersonInCharge, string? LogoUrl, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

// ── Validator ───────────────────────────────────────────────────
public class UpdateInsurerValidator : AbstractValidator<UpdateInsurerCommand>
{
    public UpdateInsurerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateInsurerCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UpdateInsurerCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(
        Guid id, UpdateInsurerCommand command, CancellationToken ct)
    {
        var insurer = await _db.Insurers.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (insurer is null)
            return Results.NotFound($"Aseguradora con ID '{id}' no encontrada.");

        var oldLogoUrl = insurer.LogoUrl;
        var logoUrl = insurer.LogoUrl;
        string? uniqueFileName = null;
        string? extension = null;

        // 1. Pre-calcular el nuevo logo si se proporciona uno
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            logoUrl = $"/uploads/insurers/{uniqueFileName}";
        }

        insurer.Name          = command.Name;
        insurer.Address       = command.Address;
        insurer.Phone         = command.Phone;
        insurer.Email         = command.Email;
        insurer.PersonInCharge = command.PersonInCharge;
        insurer.LogoUrl       = logoUrl;
        insurer.IsActive      = command.IsActive;

        // 2. Guardar en base de datos primero
        await _db.SaveChangesAsync(ct);

        // 3. Físicamente guardar el archivo localmente si la base de datos tuvo éxito
        if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
        {
            try
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "insurers");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.Photo.CopyToAsync(stream, ct);
                }

                // Borrar el archivo viejo de logo si existe
                if (!string.IsNullOrWhiteSpace(oldLogoUrl))
                {
                    var oldFilePath = Path.Combine(webRootPath, oldLogoUrl.TrimStart('/'));
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
                // Rollback: revertir el logo en base de datos
                insurer.LogoUrl = oldLogoUrl;
                await _db.SaveChangesAsync(ct);

                return Results.Problem(
                    title: "Error al almacenar el nuevo logo de la aseguradora",
                    detail: "La aseguradora no pudo ser actualizada debido a un problema físico al intentar guardar su logo en el servidor.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Ok(new UpdateInsurerResponse(
            insurer.Id, insurer.Name, insurer.Address, insurer.Phone, insurer.Email,
            insurer.PersonInCharge, insurer.LogoUrl, insurer.IsActive,
            insurer.CreatedAt, insurer.UpdatedAt));
    }
}
