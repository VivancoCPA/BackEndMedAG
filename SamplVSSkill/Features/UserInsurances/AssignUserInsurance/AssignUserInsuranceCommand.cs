using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.UserInsurances.AssignUserInsurance;

// ── Request / Response ──────────────────────────────────────────
public record AssignUserInsuranceCommand(Guid InsurerId);
public record AssignUserInsuranceResponse(string UserId, Guid InsurerId, string InsurerName, DateTime CreatedAt);

// ── Validator ───────────────────────────────────────────────────
public class AssignUserInsuranceValidator : AbstractValidator<AssignUserInsuranceCommand>
{
    public AssignUserInsuranceValidator()
    {
        RuleFor(x => x.InsurerId).NotEmpty();
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class AssignUserInsuranceCommandHandler
{
    private readonly AppDbContext _db;
    public AssignUserInsuranceCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(string userId, AssignUserInsuranceCommand command, CancellationToken ct)
    {
        // Verificar que el usuario existe
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        // Verificar que el asegurador existe
        var insurer = await _db.Insurers.FirstOrDefaultAsync(i => i.Id == command.InsurerId, ct);
        if (insurer is null)
            return Results.NotFound($"Asegurador '{command.InsurerId}' no encontrado.");

        // Evitar duplicados
        var alreadyAssigned = await _db.UserInsurances
            .AnyAsync(ui => ui.UserId == userId && ui.InsurerId == command.InsurerId, ct);

        if (alreadyAssigned)
            return Results.Conflict($"El asegurador '{insurer.Name}' ya está asignado a este usuario.");

        var userInsurance = new UserInsurance
        {
            UserId     = userId,
            InsurerId  = command.InsurerId,
            CreatedAt  = DateTime.UtcNow
        };

        _db.UserInsurances.Add(userInsurance);
        await _db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/users/{userId}/insurances",
            new AssignUserInsuranceResponse(userId, command.InsurerId, insurer.Name, userInsurance.CreatedAt));
    }
}
