using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.ToggleFamilyExtraMembershipStatus;

// ── Response ────────────────────────────────────────────────────
public record ToggleFamilyExtraMembershipStatusResponse(int Id, string FullName, bool IsActive, string Status);

// ── Command Handler (EF Core) ───────────────────────────────────
public class ToggleFamilyExtraMembershipStatusCommandHandler
{
    private readonly AppDbContext _db;
    public ToggleFamilyExtraMembershipStatusCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(Guid familyGroupId, int id, CancellationToken ct)
    {
        // 1. Verificar si el grupo familiar existe
        var groupExists = await _db.FamilyGroups.AnyAsync(g => g.Id == familyGroupId, ct);
        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        // 2. Buscar el miembro extra
        var extraMember = await _db.FamilyExtraMemberships
            .FirstOrDefaultAsync(m => m.Id == id && m.FamilyGroupId == familyGroupId, ct);

        if (extraMember is null)
            return Results.NotFound($"Miembro extra con ID '{id}' no encontrado en el grupo familiar '{familyGroupId}'.");

        // 3. Alternar estado activo/inactivo
        extraMember.IsActive = !extraMember.IsActive;
        await _db.SaveChangesAsync(ct);

        var status = extraMember.IsActive ? "Activado" : "Desactivado";
        return Results.Ok(new ToggleFamilyExtraMembershipStatusResponse(
            extraMember.Id, extraMember.FullName, extraMember.IsActive, status));
    }
}
