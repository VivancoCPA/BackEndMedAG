using FluentValidation;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.DoctorAffiliations.UpdateDoctorAffiliation;

public record UpdateDoctorAffiliationCommand(
    string? OfficeNumber,
    string? WorkSchedule)
{
    internal int Id { get; set; }
}

public class UpdateDoctorAffiliationValidator : AbstractValidator<UpdateDoctorAffiliationCommand>
{
    public UpdateDoctorAffiliationValidator()
    {
        // Add rules if needed
    }
}

public class UpdateDoctorAffiliationCommandHandler
{
    private readonly AppDbContext _context;

    public UpdateDoctorAffiliationCommandHandler(AppDbContext context) =>
        _context = context;

    public async Task<bool> HandleAsync(UpdateDoctorAffiliationCommand command, CancellationToken ct)
    {
        var affiliation = await _context.DoctorAffiliations.FindAsync(new object[] { command.Id }, ct);
        if (affiliation == null) return false;

        affiliation.OfficeNumber = command.OfficeNumber;
        affiliation.WorkSchedule = command.WorkSchedule;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}
