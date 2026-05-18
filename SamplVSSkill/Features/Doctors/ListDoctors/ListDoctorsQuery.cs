using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.ListDoctors;

// ── Response ────────────────────────────────────────────────────
public class AffiliatedCenterItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ListDoctorsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public bool IsVet { get; set; }
    public List<AffiliatedCenterItem> Centers { get; set; } = new();
}

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListDoctorsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListDoctorsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListDoctorsResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT d.id AS Id, d.name AS Name, d.last_name AS LastName, s.name AS Specialty, d.is_vet AS IsVet,
                   mc.id AS Id, mc.name AS Name
            FROM doctors d
            LEFT JOIN specialties s ON d.specialty_id = s.id
            LEFT JOIN doctor_affiliations da ON d.id = da.doctor_id
            LEFT JOIN medical_centers mc ON da.center_id = mc.id
            ORDER BY d.name, d.last_name
            """;

        var doctorDict = new Dictionary<Guid, ListDoctorsResponse>();

        var result = await connection.QueryAsync<ListDoctorsResponse, AffiliatedCenterItem, ListDoctorsResponse>(
            new CommandDefinition(sql, cancellationToken: ct),
            (doctor, center) =>
            {
                if (!doctorDict.TryGetValue(doctor.Id, out var currentDoctor))
                {
                    currentDoctor = doctor;
                    doctorDict.Add(currentDoctor.Id, currentDoctor);
                }

                if (center != null && center.Id != Guid.Empty)
                {
                    currentDoctor.Centers.Add(center);
                }
                return currentDoctor;
            },
            splitOn: "Id"
        );

        return doctorDict.Values;
    }
}
