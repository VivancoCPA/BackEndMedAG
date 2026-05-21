using FluentValidation;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyGroups.CreateFamilyGroup;

// ── Request / Response ──────────────────────────────────────────
public record CreateFamilyGroupCommand(
    string Name,
    string? UserId,
    string? PhotoUrl);

public record CreateFamilyGroupResponse(Guid Id, string Name, string? UserId, bool IsActive, DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class CreateFamilyGroupValidator : AbstractValidator<CreateFamilyGroupCommand>
{
    public CreateFamilyGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class CreateFamilyGroupCommandHandler
{
    private readonly AppDbContext _db;
    public CreateFamilyGroupCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(CreateFamilyGroupCommand command, CancellationToken ct)
    {
        var group = new FamilyGroup
        {
            Id        = Guid.NewGuid(),
            Name      = command.Name,
            UserId    = command.UserId,
            PhotoUrl  = command.PhotoUrl,
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.FamilyGroups.Add(group);
        await _db.SaveChangesAsync(ct);

        return Results.Created($"/api/family-groups/{group.Id}",
            new CreateFamilyGroupResponse(group.Id, group.Name, group.UserId, group.IsActive, group.CreatedAt));
    }
}
