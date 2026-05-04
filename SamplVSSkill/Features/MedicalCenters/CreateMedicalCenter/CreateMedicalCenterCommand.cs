using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Domain.Enums;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.CreateMedicalCenter;

// ── Request / Response ──────────────────────────────────────────
public record CreateMedicalCenterCommand(
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

public record CreateMedicalCenterResponse(
    Guid Id,
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

// ── Command Handler (EF Core) ───────────────────────────────────
public class CreateMedicalCenterCommandHandler
{
    private readonly AppDbContext _db;

    public CreateMedicalCenterCommandHandler(AppDbContext db) => _db = db;

    public async Task<CreateMedicalCenterResponse> HandleAsync(CreateMedicalCenterCommand command, CancellationToken ct)
    {
        var center = new MedicalCenter
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Type = command.Type,
            Address = command.Address,
            Phone = command.Phone,
            IsActive = command.IsActive,
            Latitude = command.Latitude,
            Longitude = command.Longitude
        };

        _db.MedicalCenters.Add(center);
        await _db.SaveChangesAsync(ct);

        return new CreateMedicalCenterResponse(
            center.Id, center.Name, center.Type,
            center.Address, center.Phone, center.IsActive,
            center.Latitude, center.Longitude);
    }
}
