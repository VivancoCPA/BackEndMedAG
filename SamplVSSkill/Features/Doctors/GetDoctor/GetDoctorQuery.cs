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
            SELECT id AS Id, name AS Name, specialty AS Specialty, is_vet AS IsVet
            FROM doctors
            WHERE id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<GetDoctorResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
