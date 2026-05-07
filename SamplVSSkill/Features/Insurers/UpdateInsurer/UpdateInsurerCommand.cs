using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Insurers.UpdateInsurer;

// ── Request / Response ──────────────────────────────────────────
public record UpdateInsurerCommand(
    string Name,
    string Address,
    string Phone,
    string Email,
    string? PersonInCharge,
    string? LogoUrl,
    bool IsActive);

public record UpdateInsurerResponse(
    Guid Id, string Name, string Address, string Phone, string Email,
    string? PersonInCharge, string? LogoUrl, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

// ── Validator ───────────────────────────────────────────────────
public class UpdateInsurerValidator : AbstractValidator<UpdateInsurerCommand>
{
    public UpdateInsurerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class UpdateInsurerCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateInsurerCommandHandler(AppDbContext db) => _db = db;

    public async Task<UpdateInsurerResponse?> HandleAsync(
        Guid id, UpdateInsurerCommand command, CancellationToken ct)
    {
        var insurer = await _db.Insurers.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (insurer is null) return null;

        insurer.Name          = command.Name;
        insurer.Address       = command.Address;
        insurer.Phone         = command.Phone;
        insurer.Email         = command.Email;
        insurer.PersonInCharge = command.PersonInCharge;
        insurer.LogoUrl       = command.LogoUrl;
        insurer.IsActive      = command.IsActive;
        // UpdatedAt set automatically by AppDbContext.SaveChangesAsync

        await _db.SaveChangesAsync(ct);

        return new UpdateInsurerResponse(
            insurer.Id, insurer.Name, insurer.Address, insurer.Phone, insurer.Email,
            insurer.PersonInCharge, insurer.LogoUrl, insurer.IsActive,
            insurer.CreatedAt, insurer.UpdatedAt);
    }
}
