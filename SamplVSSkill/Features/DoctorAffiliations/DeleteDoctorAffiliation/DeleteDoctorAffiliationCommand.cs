using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.DoctorAffiliations.DeleteDoctorAffiliation;

public record DeleteDoctorAffiliationCommand(int Id);

public class DeleteDoctorAffiliationCommandHandler
{
    private readonly AppDbContext _context;

    public DeleteDoctorAffiliationCommandHandler(AppDbContext context) =>
        _context = context;

    public async Task<bool> HandleAsync(DeleteDoctorAffiliationCommand command, CancellationToken ct)
    {
        var affiliation = await _context.DoctorAffiliations.FindAsync(new object[] { command.Id }, ct);
        if (affiliation == null) return false;

        _context.DoctorAffiliations.Remove(affiliation);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
