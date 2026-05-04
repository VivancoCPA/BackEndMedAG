using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

// ── Request / Response ──────────────────────────────────────────
public record UpdateDoctorCommand(string Name, string? Specialty, bool IsVet);
public record UpdateDoctorResponse(Guid Id, string Name, string? Specialty, bool IsVet);

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateDoctorCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateDoctorCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateDoctorResponse?> HandleAsync(Guid id, UpdateDoctorCommand command, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);

        if (doctor is null)
            return null;

        doctor.Name = command.Name;
        doctor.Specialty = command.Specialty;
        doctor.IsVet = command.IsVet;

        await _db.SaveChangesAsync(ct);

        return new UpdateDoctorResponse(doctor.Id, doctor.Name, doctor.Specialty, doctor.IsVet);
    }
}
