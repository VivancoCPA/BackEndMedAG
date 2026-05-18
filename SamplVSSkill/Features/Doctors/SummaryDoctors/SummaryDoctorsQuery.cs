using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.SummaryDoctors;

public record DoctorSummaryResponse(
    long Total,
    long Active,
    long Inactive);

public class SummaryDoctorsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public SummaryDoctorsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<DoctorSummaryResponse> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                COUNT(*)                              AS Total,
                COUNT(*) FILTER (WHERE is_active)     AS Active,
                COUNT(*) FILTER (WHERE NOT is_active) AS Inactive
            FROM doctors
            """;

        return await connection.QuerySingleAsync<DoctorSummaryResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
