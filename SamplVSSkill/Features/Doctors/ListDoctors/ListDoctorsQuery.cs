using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.ListDoctors;

// ── Response ────────────────────────────────────────────────────
public record ListDoctorsResponse(Guid Id, string Name, string? Specialty, bool IsVet);

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
            SELECT id AS Id, name AS Name, specialty AS Specialty, is_vet AS IsVet
            FROM doctors
            ORDER BY name
            """;

        return await connection.QueryAsync<ListDoctorsResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
