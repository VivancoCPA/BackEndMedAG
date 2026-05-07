using FluentValidation;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Insurers.CreateInsurer;

// ── Request / Response ──────────────────────────────────────────
public record CreateInsurerCommand(
    string Name,
    string Address,
    string Phone,
    string Email,
    string? PersonInCharge,
    string? LogoUrl);

public record CreateInsurerResponse(
    Guid Id, string Name, string Address, string Phone, string Email,
    string? PersonInCharge, string? LogoUrl, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

// ── Validator ───────────────────────────────────────────────────
public class CreateInsurerValidator : AbstractValidator<CreateInsurerCommand>
{
    public CreateInsurerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

// ── Command Handler (EF Core) ───────────────────────────────────
public class CreateInsurerCommandHandler
{
    private readonly AppDbContext _db;

    public CreateInsurerCommandHandler(AppDbContext db) => _db = db;

    public async Task<CreateInsurerResponse> HandleAsync(
        CreateInsurerCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var insurer = new Insurer
        {
            Id            = Guid.CreateVersion7(),
            Name          = command.Name,
            Address       = command.Address,
            Phone         = command.Phone,
            Email         = command.Email,
            PersonInCharge = command.PersonInCharge,
            LogoUrl       = command.LogoUrl,
            IsActive      = true,
            CreatedAt     = now,
            UpdatedAt     = now
        };

        _db.Insurers.Add(insurer);
        await _db.SaveChangesAsync(ct);

        return new CreateInsurerResponse(
            insurer.Id, insurer.Name, insurer.Address, insurer.Phone, insurer.Email,
            insurer.PersonInCharge, insurer.LogoUrl, insurer.IsActive,
            insurer.CreatedAt, insurer.UpdatedAt);
    }
}
