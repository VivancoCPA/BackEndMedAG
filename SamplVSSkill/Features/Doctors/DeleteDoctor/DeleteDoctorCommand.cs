using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.DeleteDoctor;

// ── Command Handler (EF Core) ───────────────────────────────────
public class DeleteDoctorCommandHandler
{
    private readonly AppDbContext _db;

    public DeleteDoctorCommandHandler(AppDbContext db) => _db = db;

    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);

        if (doctor is null)
            return false;

        _db.Doctors.Remove(doctor);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}
