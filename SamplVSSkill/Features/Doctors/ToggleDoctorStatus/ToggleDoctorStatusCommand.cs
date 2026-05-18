using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.ToggleDoctorStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleDoctorStatusResponse(Guid Id, string Name, string LastName, bool IsActive, string Status);

// ── Command Handler (EF Core) ───────────────────────────────────
public class ToggleDoctorStatusCommandHandler
{
    private readonly AppDbContext _db;

    public ToggleDoctorStatusCommandHandler(AppDbContext db) => _db = db;

    public async Task<ToggleDoctorStatusResponse?> HandleAsync(Guid id, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doctor is null) return null;

        // Toggle active status
        doctor.IsActive = !doctor.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = doctor.IsActive ? "Activado" : "Inactivado";
        return new ToggleDoctorStatusResponse(doctor.Id, doctor.Name, doctor.LastName, doctor.IsActive, status);
    }
}
