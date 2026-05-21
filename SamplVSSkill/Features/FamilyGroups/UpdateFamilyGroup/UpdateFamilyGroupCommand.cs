using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyGroups.UpdateFamilyGroup;

// ── Request / Response ──────────────────────────────────────────
public record UpdateFamilyGroupCommand(string Name, string? UserId, string? PhotoUrl);
public record UpdateFamilyGroupResponse(Guid Id, string Name, string? UserId, string? PhotoUrl, bool IsActive);

// ── Validator ───────────────────────────────────────────────────
public class UpdateFamilyGroupValidator : AbstractValidator<UpdateFamilyGroupCommand>
{
    public UpdateFamilyGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateFamilyGroupCommandHandler
{
    private readonly AppDbContext _db;
    public UpdateFamilyGroupCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(Guid id, UpdateFamilyGroupCommand command, CancellationToken ct)
    {
        var group = await _db.FamilyGroups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return Results.NotFound($"Grupo familiar '{id}' no encontrado.");

        group.Name     = command.Name;
        group.UserId   = command.UserId;
        group.PhotoUrl = command.PhotoUrl;

        await _db.SaveChangesAsync(ct);

        return Results.Ok(new UpdateFamilyGroupResponse(group.Id, group.Name, group.UserId, group.PhotoUrl, group.IsActive));
    }
}
