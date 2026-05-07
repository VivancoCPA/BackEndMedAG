using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

// ── Request / Response ──────────────────────────────────────────
public record UpdateDoctorCommand(
    string Name,
    string LastName,
    int? SpecialtyId,
    string? Register,
    string? Phone,
    string? Email,
    string? PhotoUrl,
    bool IsVet,
    bool IsActive);

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

    public UpdateDoctorCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateDoctorResponse?> HandleAsync(
        Guid id, UpdateDoctorCommand command, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doctor is null) return null;

        doctor.Name        = command.Name;
        doctor.LastName    = command.LastName;
        doctor.SpecialtyId = command.SpecialtyId;
        doctor.Register    = command.Register;
        doctor.Phone       = command.Phone;
        doctor.Email       = command.Email;
        doctor.PhotoUrl    = command.PhotoUrl;
        doctor.IsVet       = command.IsVet;
        doctor.IsActive    = command.IsActive;
        // UpdatedAt set automatically by AppDbContext.SaveChangesAsync

        await _db.SaveChangesAsync(ct);

        return new UpdateDoctorResponse(
            doctor.Id, doctor.Name, doctor.LastName, doctor.SpecialtyId,
            doctor.Register, doctor.Phone, doctor.Email, doctor.PhotoUrl,
            doctor.IsVet, doctor.IsActive, doctor.CreatedAt, doctor.UpdatedAt);
    }
}
