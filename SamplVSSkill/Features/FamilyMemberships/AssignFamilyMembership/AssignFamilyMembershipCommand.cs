using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyMemberships.AssignFamilyMembership;

// ── Request / Response ──────────────────────────────────────────
public record AssignFamilyMembershipCommand(string UserId, bool IsAdmin, string Relationship);
public record AssignFamilyMembershipResponse(int Id, string UserId, Guid FamilyGroupId, bool IsAdmin, string? Relationship);

// ── Validator ───────────────────────────────────────────────────
public class AssignFamilyMembershipValidator : AbstractValidator<AssignFamilyMembershipCommand>
{
    public AssignFamilyMembershipValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido.");

        RuleFor(x => x.Relationship)
            .NotEmpty().WithMessage("El tipo de relación es requerido.")
            .MaximumLength(100).WithMessage("La relación no puede exceder 100 caracteres.");
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class AssignFamilyMembershipCommandHandler
{
    private readonly AppDbContext _db;
    public AssignFamilyMembershipCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(Guid familyGroupId, AssignFamilyMembershipCommand command, CancellationToken ct)
    {
        // Verificar que el grupo familiar existe
        var groupExists = await _db.FamilyGroups.AnyAsync(fg => fg.Id == familyGroupId, ct);
        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        // Verificar que el usuario existe
        var userExists = await _db.Users.AnyAsync(u => u.Id == command.UserId, ct);
        if (!userExists)
            return Results.NotFound($"Usuario '{command.UserId}' no encontrado.");

        // Evitar que el usuario pertenezca a más de un grupo familiar
        var alreadyMemberOfAnyGroup = await _db.FamilyMemberships
            .AnyAsync(m => m.UserId == command.UserId, ct);

        if (alreadyMemberOfAnyGroup)
            return Results.Conflict($"El usuario ya pertenece a un grupo familiar.");

        var membership = new FamilyMembership
        {
            FamilyGroupId = familyGroupId,
            UserId = command.UserId,
            IsAdmin = command.IsAdmin,
            Relationship = command.Relationship
        };

        _db.FamilyMemberships.Add(membership);
        await _db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/family-groups/{familyGroupId}/members",
            new AssignFamilyMembershipResponse(membership.Id, membership.UserId, membership.FamilyGroupId, membership.IsAdmin, membership.Relationship));
    }
}
