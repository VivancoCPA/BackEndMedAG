using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.DeleteMedicalCenter;

// ── Response ────────────────────────────────────────────────────
public record ToggleMedicalCenterStatusResponse(Guid Id, string Name, bool IsActive, string Status);

// ── Command Handler (EF Core) — Toggle Status ──────────────────
public class DeleteMedicalCenterCommandHandler
{
    private readonly AppDbContext _db;

    public DeleteMedicalCenterCommandHandler(AppDbContext db) => _db = db;

    public async Task<ToggleMedicalCenterStatusResponse?> HandleAsync(Guid id, CancellationToken ct)
    {
        var center = await _db.MedicalCenters.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (center is null)
            return null;

        // Toggle — if active → deactivate, if inactive → reactivate
        center.IsActive = !center.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = center.IsActive ? "Activado" : "Inactivado";
        return new ToggleMedicalCenterStatusResponse(center.Id, center.Name, center.IsActive, status);
    }
}
