using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.CenterTypes.ToggleCenterTypeStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleCenterTypeStatusResponse(int Id, string Name, bool IsActive, string Status);

// ── Command Handler (EF Core) ───────────────────────────────────
public class ToggleCenterTypeStatusCommandHandler
{
    private readonly AppDbContext _db;

    public ToggleCenterTypeStatusCommandHandler(AppDbContext db) => _db = db;

    public async Task<ToggleCenterTypeStatusResponse?> HandleAsync(int id, CancellationToken ct)
    {
        var centerType = await _db.CenterTypes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (centerType is null) return null;

        // Master table — never delete, only toggle IsActive
        centerType.IsActive = !centerType.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = centerType.IsActive ? "Activado" : "Inactivado";
        return new ToggleCenterTypeStatusResponse(centerType.Id, centerType.Name, centerType.IsActive, status);
    }
}
