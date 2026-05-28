using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyMemberships.RemoveFamilyMembership;

// ── Response ────────────────────────────────────────────────────
public record RemoveFamilyMembershipResponse(string UserId, Guid FamilyGroupId, string Message);

// ── Command Handler (EF Core) ───────────────────────────────────
public class RemoveFamilyMembershipCommandHandler
{
    private readonly AppDbContext _db;
    public RemoveFamilyMembershipCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(Guid familyGroupId, string userId, CancellationToken ct)
    {
        var record = await _db.FamilyMemberships
            .FirstOrDefaultAsync(m => m.FamilyGroupId == familyGroupId && m.UserId == userId, ct);

        if (record is null)
            return Results.NotFound($"El usuario '{userId}' no es miembro del grupo familiar '{familyGroupId}'.");

        _db.FamilyMemberships.Remove(record);
        await _db.SaveChangesAsync(ct);

        return Results.Ok(new RemoveFamilyMembershipResponse(
            userId, familyGroupId, "Miembro desvinculado del grupo familiar exitosamente."));
    }
}
