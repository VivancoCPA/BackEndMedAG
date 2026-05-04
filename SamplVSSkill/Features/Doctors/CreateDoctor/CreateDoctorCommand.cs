using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.CreateDoctor;

// ── Request / Response ──────────────────────────────────────────
public record CreateDoctorCommand(string Name, string? Specialty, bool IsVet);
public record CreateDoctorResponse(Guid Id, string Name, string? Specialty, bool IsVet);

// ── Command Handler (EF Core) ───────────────────────────────────
public class CreateDoctorCommandHandler
{
    private readonly AppDbContext _db;

    public CreateDoctorCommandHandler(AppDbContext db) => _db = db;

    public async Task<CreateDoctorResponse> HandleAsync(CreateDoctorCommand command, CancellationToken ct)
    {
        var doctor = new Doctor
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Specialty = command.Specialty,
            IsVet = command.IsVet
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(ct);

        return new CreateDoctorResponse(doctor.Id, doctor.Name, doctor.Specialty, doctor.IsVet);
    }
}
