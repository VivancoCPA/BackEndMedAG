using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.GetDoctor;

// ── Response ────────────────────────────────────────────────────
public record GetDoctorResponse(Guid Id, string Name, string? Specialty, bool IsVet);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class GetDoctorQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public GetDoctorQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<GetDoctorResponse?> HandleAsync(Guid id, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT d.id AS Id,
                   d.name AS Name,
                   s.name AS Specialty,
                   d.is_vet AS IsVet
            FROM doctors d
            LEFT JOIN specialties s ON d.specialty_id = s.id
            WHERE d.id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<GetDoctorResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
