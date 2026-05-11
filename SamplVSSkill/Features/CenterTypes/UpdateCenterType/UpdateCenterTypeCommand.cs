using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.CenterTypes.UpdateCenterType;

// ── Request / Response ──────────────────────────────────────────
public record UpdateCenterTypeCommand(string Name);
public record UpdateCenterTypeResponse(int Id, string Name, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

// ── Validator ───────────────────────────────────────────────────
public class UpdateCenterTypeValidator : AbstractValidator<UpdateCenterTypeCommand>
{
    public UpdateCenterTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del tipo es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateCenterTypeCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateCenterTypeCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateCenterTypeResponse?> HandleAsync(
        int id, UpdateCenterTypeCommand command, CancellationToken ct)
    {
        var centerType = await _db.CenterTypes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (centerType is null) return null;

        centerType.Name = command.Name;
        // UpdatedAt set automatically by AppDbContext.SaveChangesAsync

        await _db.SaveChangesAsync(ct);

        return new UpdateCenterTypeResponse(
            centerType.Id, centerType.Name, centerType.IsActive,
            centerType.CreatedAt, centerType.UpdatedAt);
    }
}
