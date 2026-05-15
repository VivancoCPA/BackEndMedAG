using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.UpdateMedicalCenter;

// ── Request / Response ──────────────────────────────────────────
public record UpdateMedicalCenterCommand(
    string Name,
    int? TypeId,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

public record UpdateMedicalCenterResponse(
    Guid Id,
    string Name,
    int? TypeId,
    string? TypeName,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateMedicalCenterCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateMedicalCenterCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateMedicalCenterResponse?> HandleAsync(
        Guid id, UpdateMedicalCenterCommand command, CancellationToken ct)
    {
        var center = await _db.MedicalCenters
            .Include(c => c.CenterType)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (center is null)
            return null;

        center.Name      = command.Name;
        center.TypeId    = command.TypeId;
        center.Address   = command.Address;
        center.Phone     = command.Phone;
        center.IsActive  = command.IsActive;
        center.Latitude  = command.Latitude;
        center.Longitude = command.Longitude;
        // UpdatedAt is set automatically by AppDbContext.SaveChangesAsync
        // CreatedAt is never modified here

        await _db.SaveChangesAsync(ct);

        return new UpdateMedicalCenterResponse(
            center.Id, center.Name,
            center.TypeId, center.CenterType?.Name,
            center.Address, center.Phone, center.IsActive,
            center.Latitude, center.Longitude,
            center.CreatedAt, center.UpdatedAt);
    }
}
