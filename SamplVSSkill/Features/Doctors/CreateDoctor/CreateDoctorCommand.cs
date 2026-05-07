using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.CreateDoctor;

// ── Request / Response ──────────────────────────────────────────
public record CreateDoctorCommand(
    string Name,
    string LastName,
    int? SpecialtyId,
    string? Register,
    string? Phone,
    string? Email,
    string? PhotoUrl,
    bool IsVet);

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

    public CreateDoctorCommandHandler(AppDbContext db) => _db = db;

    public async Task<CreateDoctorResponse> HandleAsync(
        CreateDoctorCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var doctor = new Doctor
        {
            Id          = Guid.CreateVersion7(),
            Name        = command.Name,
            LastName    = command.LastName,
            SpecialtyId = command.SpecialtyId,
            Register    = command.Register,
            Phone       = command.Phone,
            Email       = command.Email,
            PhotoUrl    = command.PhotoUrl,
            IsVet       = command.IsVet,
            IsActive    = true,
            CreatedAt   = now,
            UpdatedAt   = now
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(ct);

        return new CreateDoctorResponse(
            doctor.Id, doctor.Name, doctor.LastName, doctor.SpecialtyId,
            doctor.Register, doctor.Phone, doctor.Email, doctor.PhotoUrl,
            doctor.IsVet, doctor.IsActive, doctor.CreatedAt, doctor.UpdatedAt);
    }
}
