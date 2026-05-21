using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.UserInsurances.RemoveUserInsurance;

// ── Response ────────────────────────────────────────────────────
public record RemoveUserInsuranceResponse(string UserId, Guid InsurerId, string Message);

// ── Command Handler (EF Core) ───────────────────────────────────
public class RemoveUserInsuranceCommandHandler
{
    private readonly AppDbContext _db;
    public RemoveUserInsuranceCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(string userId, Guid insurerId, CancellationToken ct)
    {
        var record = await _db.UserInsurances
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.InsurerId == insurerId, ct);

        if (record is null)
            return Results.NotFound($"El asegurador '{insurerId}' no está asignado al usuario '{userId}'.");

        _db.UserInsurances.Remove(record);
        await _db.SaveChangesAsync(ct);

        return Results.Ok(new RemoveUserInsuranceResponse(
            userId, insurerId, "Asegurador desvinculado exitosamente."));
    }
}
