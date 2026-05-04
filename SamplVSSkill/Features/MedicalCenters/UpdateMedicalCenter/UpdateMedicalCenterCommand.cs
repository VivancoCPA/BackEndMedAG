using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Enums;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.UpdateMedicalCenter;

// ── Request / Response ──────────────────────────────────────────
public record UpdateMedicalCenterCommand(
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

public record UpdateMedicalCenterResponse(
    Guid Id,
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateMedicalCenterCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateMedicalCenterCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateMedicalCenterResponse?> HandleAsync(Guid id, UpdateMedicalCenterCommand command, CancellationToken ct)
    {
        var center = await _db.MedicalCenters.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (center is null)
            return null;

        center.Name = command.Name;
        center.Type = command.Type;
        center.Address = command.Address;
        center.Phone = command.Phone;
        center.IsActive = command.IsActive;
        center.Latitude = command.Latitude;
        center.Longitude = command.Longitude;

        await _db.SaveChangesAsync(ct);

        return new UpdateMedicalCenterResponse(
            center.Id, center.Name, center.Type,
            center.Address, center.Phone, center.IsActive,
            center.Latitude, center.Longitude);
    }
}
