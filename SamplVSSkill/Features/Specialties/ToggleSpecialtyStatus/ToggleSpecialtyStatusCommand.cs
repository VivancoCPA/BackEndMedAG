using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Specialties.ToggleSpecialtyStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleSpecialtyStatusResponse(int Id, string Name, bool IsActive, string Status);

// ── Command Handler (EF Core) ───────────────────────────────────
public class ToggleSpecialtyStatusCommandHandler
{
    private readonly AppDbContext _db;

    public ToggleSpecialtyStatusCommandHandler(AppDbContext db) => _db = db;

    public async Task<ToggleSpecialtyStatusResponse?> HandleAsync(int id, CancellationToken ct)
    {
        var specialty = await _db.Specialties.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (specialty is null) return null;

        // Master table — never delete, only toggle
        specialty.IsActive = !specialty.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = specialty.IsActive ? "Activado" : "Inactivado";
        return new ToggleSpecialtyStatusResponse(specialty.Id, specialty.Name, specialty.IsActive, status);
    }
}
