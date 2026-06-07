using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Doctors.CreateDoctor;

// ── Request / Response ──────────────────────────────────────────
public record DoctorAffiliationRequest
{
    public Guid Id { get; init; }
    public string? OfficeNumber { get; init; }
    public string? WorkSchedule { get; init; }
}

public record CreateDoctorCommand
{
    public string Name { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public int? SpecialtyId { get; init; }
    public string? Register { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public IFormFile? Photo { get; init; }
    public bool IsVet { get; init; }
    public List<DoctorAffiliationRequest>? Centers { get; init; }
}

public record CreateDoctorResponse(
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
public class CreateDoctorCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateDoctorCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(
        CreateDoctorCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        string? photoUrl = null;
        string? extension = null;
        string? uniqueFileName = null;

        // 1. Pre-calcular el nombre de archivo y la ruta de la foto si se proporciona una
        if (command.Photo is not null && command.Photo.Length > 0)
        {
            extension = Path.GetExtension(command.Photo.FileName);
            uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            photoUrl = $"/uploads/doctors/{uniqueFileName}";
        }

        // Iniciar transacción de base de datos para atomicidad perfecta
        using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var doctor = new Doctor
            {
                Id          = Guid.CreateVersion7(),
                Name        = command.Name,
                LastName    = command.LastName,
                SpecialtyId = command.SpecialtyId,
                Register    = command.Register,
                Phone       = command.Phone,
                Email       = command.Email,
                PhotoUrl    = photoUrl,
                IsVet       = command.IsVet,
                IsActive    = true,
                CreatedAt   = now,
                UpdatedAt   = now
            };

            _db.Doctors.Add(doctor);

            if (command.Centers?.Any() == true)
            {
                foreach (var aff in command.Centers)
                {
                    _db.DoctorAffiliations.Add(new DoctorAffiliation
                    {
                        DoctorId = doctor.Id,
                        CenterId = aff.Id,
                        OfficeNumber = aff.OfficeNumber,
                        WorkSchedule = aff.WorkSchedule,
                        CreatedAt = now
                    });
                }
            }

            // 2. Guardar en base de datos primero (dentro de la transacción)
            await _db.SaveChangesAsync(ct);

            // 3. Físicamente guardar el archivo localmente
            if (command.Photo is not null && command.Photo.Length > 0 && uniqueFileName is not null)
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
            }

            // Confirmar transacción de base de datos si todo salió bien
            await transaction.CommitAsync(ct);

            return Results.Created(
                $"/api/doctors/{doctor.Id}",
                new CreateDoctorResponse(
                    doctor.Id, doctor.Name, doctor.LastName, doctor.SpecialtyId,
                    doctor.Register, doctor.Phone, doctor.Email, doctor.PhotoUrl,
                    doctor.IsVet, doctor.IsActive, doctor.CreatedAt, doctor.UpdatedAt));
        }
        catch (Exception)
        {
            // Rollback de base de datos
            await transaction.RollbackAsync(ct);

            // Limpieza de archivo físico si se llegó a crear antes del fallo de commit
            if (uniqueFileName is not null)
            {
                try
                {
                    var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var filePath = Path.Combine(webRootPath, "uploads", "doctors", uniqueFileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch
                {
                    // Ignorar error al limpiar
                }
            }

            return Results.Problem(
                title: "Error al registrar el doctor",
                detail: "El doctor no pudo ser creado debido a un problema en la base de datos o en el almacenamiento físico.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
