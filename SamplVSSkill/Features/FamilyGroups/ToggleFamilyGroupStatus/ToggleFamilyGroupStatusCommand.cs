using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyGroups.ToggleFamilyGroupStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleFamilyGroupStatusResponse(Guid Id, string Name, bool IsActive, string Status);

// ── Command Handler (EF Core) ───────────────────────────────────
public class ToggleFamilyGroupStatusCommandHandler
{
    private readonly AppDbContext _db;
    public ToggleFamilyGroupStatusCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(Guid id, CancellationToken ct)
    {
        var group = await _db.FamilyGroups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return Results.NotFound($"Grupo familiar '{id}' no encontrado.");

        group.IsActive = !group.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = group.IsActive ? "Activado" : "Desactivado";
        return Results.Ok(new ToggleFamilyGroupStatusResponse(group.Id, group.Name, group.IsActive, status));
    }
}
