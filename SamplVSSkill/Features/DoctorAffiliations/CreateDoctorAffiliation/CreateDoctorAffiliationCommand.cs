using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.DoctorAffiliations.CreateDoctorAffiliation;

public record CreateDoctorAffiliationCommand(
    Guid DoctorId,
    Guid CenterId,
    string? OfficeNumber,
    string? WorkSchedule);

public class CreateDoctorAffiliationValidator : AbstractValidator<CreateDoctorAffiliationCommand>
{
    public CreateDoctorAffiliationValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.CenterId).NotEmpty();
    }
}

public class CreateDoctorAffiliationCommandHandler
{
    private readonly AppDbContext _context;

    public CreateDoctorAffiliationCommandHandler(AppDbContext context) =>
        _context = context;

    public async Task<int> HandleAsync(CreateDoctorAffiliationCommand command, CancellationToken ct)
    {
        var exists = await _context.DoctorAffiliations
            .AnyAsync(x => x.DoctorId == command.DoctorId && x.CenterId == command.CenterId, ct);
        
        if (exists)
            throw new InvalidOperationException("El médico ya está afiliado a este centro.");

        var affiliation = new DoctorAffiliation
        {
            DoctorId = command.DoctorId,
            CenterId = command.CenterId,
            OfficeNumber = command.OfficeNumber,
            WorkSchedule = command.WorkSchedule,
            CreatedAt = DateTime.UtcNow
        };

        _context.DoctorAffiliations.Add(affiliation);
        await _context.SaveChangesAsync(ct);

        return affiliation.Id;
    }
}
