using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Insurers.ToggleInsurerStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleInsurerStatusResponse(Guid Id, string Name, bool IsActive, string Status);

// ── Command Handler (EF Core) ───────────────────────────────────
public class ToggleInsurerStatusCommandHandler
{
    private readonly AppDbContext _db;

    public ToggleInsurerStatusCommandHandler(AppDbContext db) => _db = db;

    public async Task<ToggleInsurerStatusResponse?> HandleAsync(Guid id, CancellationToken ct)
    {
        var insurer = await _db.Insurers.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (insurer is null) return null;

        // Master table — never delete, only toggle
        insurer.IsActive = !insurer.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = insurer.IsActive ? "Activado" : "Inactivado";
        return new ToggleInsurerStatusResponse(insurer.Id, insurer.Name, insurer.IsActive, status);
    }
}
