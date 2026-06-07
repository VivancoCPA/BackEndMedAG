using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

// ── Request / Response ──────────────────────────────────────────
public record DoctorAffiliationRequest
{
    public Guid Id { get; init; }
    public string? OfficeNumber { get; init; }
    public string? WorkSchedule { get; init; }
}

public record UpdateDoctorCommand
{
    public string Name { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public int? SpecialtyId { get; init; }
    public string? Register { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public IFormFile? Photo { get; init; }
    public bool IsVet { get; init; }
    public bool IsActive { get; init; }
    public List<DoctorAffiliationRequest>? Centers { get; init; }
}

public record UpdateDoctorResponse(
    Guid Id,
    string Name,
    string LastName,
    int? SpecialtyId,
    string? Register,
    string? Phone,
    string? Email,
    string? PhotoUrl,
    bool IsVet,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateDoctorCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UpdateDoctorCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(
        Guid id, UpdateDoctorCommand command, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doctor is null)
            return Results.NotFound($"Doctor con ID '{id}' no encontrado.");

        var oldPhotoUrl = doctor.PhotoUrl;
        var photoUrl = doctor.PhotoUrl;
        string? uniqueFileName = null;
        string? extension = null;

        // 1. Pre-calcular el nuevo path si se proporciona una nueva foto
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            photoUrl = $"/uploads/doctors/{uniqueFileName}";
        }

        doctor.Name        = command.Name;
        doctor.LastName    = command.LastName;
        doctor.SpecialtyId = command.SpecialtyId;
        doctor.Register    = command.Register;
        doctor.Phone       = command.Phone;
        doctor.Email       = command.Email;
        doctor.PhotoUrl    = photoUrl;
        doctor.IsVet       = command.IsVet;
        doctor.IsActive    = command.IsActive;

        // Sincronizar afiliaciones
        var existingAffiliations = await _db.DoctorAffiliations.Where(a => a.DoctorId == doctor.Id).ToListAsync(ct);
        var requestedCenters = command.Centers ?? new List<DoctorAffiliationRequest>();

        var toRemove = existingAffiliations.Where(e => !requestedCenters.Any(r => r.Id == e.CenterId)).ToList();
        _db.DoctorAffiliations.RemoveRange(toRemove);

        foreach (var req in requestedCenters)
        {
            var existing = existingAffiliations.FirstOrDefault(e => e.CenterId == req.Id);
            if (existing != null)
            {
                existing.OfficeNumber = req.OfficeNumber;
                existing.WorkSchedule = req.WorkSchedule;
            }
            else
            {
                _db.DoctorAffiliations.Add(new SamplVSSkill.Domain.Entities.DoctorAffiliation
                {
                    DoctorId = doctor.Id,
                    CenterId = req.Id,
                    OfficeNumber = req.OfficeNumber,
                    WorkSchedule = req.WorkSchedule,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // 2. Guardar en base de datos primero
        await _db.SaveChangesAsync(ct);

        // 3. Físicamente guardar el archivo localmente si la base de datos tuvo éxito
        if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
        {
            try
            {
                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "doctors");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.Photo.CopyToAsync(stream, ct);
                }

                // Borrar el archivo viejo de foto si existía y era válido
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
                // Rollback: revertir el PhotoUrl en base de datos
                doctor.PhotoUrl = oldPhotoUrl;
                await _db.SaveChangesAsync(ct);

                return Results.Problem(
                    title: "Error al almacenar la nueva foto del doctor",
                    detail: "El doctor no pudo ser actualizado debido a un problema físico al intentar guardar su foto en el servidor.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Results.Ok(new UpdateDoctorResponse(
            doctor.Id, doctor.Name, doctor.LastName, doctor.SpecialtyId,
            doctor.Register, doctor.Phone, doctor.Email, doctor.PhotoUrl,
            doctor.IsVet, doctor.IsActive, doctor.CreatedAt, doctor.UpdatedAt));
    }
}
