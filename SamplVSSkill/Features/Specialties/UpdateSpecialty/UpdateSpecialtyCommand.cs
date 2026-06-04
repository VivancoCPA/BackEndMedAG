using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Specialties.UpdateSpecialty;

// ── Request / Response ──────────────────────────────────────────
public record UpdateSpecialtyCommand(string Name, string? Description, bool IsActive);
public record UpdateSpecialtyResponse(int Id, string Name, string Description, bool IsActive, DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class UpdateSpecialtyValidator : AbstractValidator<UpdateSpecialtyCommand>
{
    public UpdateSpecialtyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la especialidad es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateSpecialtyCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateSpecialtyCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateSpecialtyResponse?> HandleAsync(
        int id, UpdateSpecialtyCommand command, CancellationToken ct)
    {
        var specialty = await _db.Specialties.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (specialty is null) return null;

        specialty.Name        = command.Name;
        specialty.Description = command.Description ?? string.Empty;
        specialty.IsActive    = command.IsActive;
        await _db.SaveChangesAsync(ct);

        return new UpdateSpecialtyResponse(
            specialty.Id, specialty.Name, specialty.Description, specialty.IsActive, specialty.CreatedAt);
    }
}
