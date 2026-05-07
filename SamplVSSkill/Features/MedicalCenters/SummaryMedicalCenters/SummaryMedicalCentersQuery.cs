using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.SummaryMedicalCenters;

// ── Response ────────────────────────────────────────────────────
// Extensible: agregar más campos de resumen aquí sin tocar el endpoint paginado
public record MedicalCenterSummaryResponse(
    long Total,
    long Active,
    long Inactive);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class SummaryMedicalCentersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public SummaryMedicalCentersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<MedicalCenterSummaryResponse> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Single query — GROUP BY replaced by FILTER for clarity and performance
        const string sql = """
            SELECT
                COUNT(*)                              AS Total,
                COUNT(*) FILTER (WHERE is_active)     AS Active,
                COUNT(*) FILTER (WHERE NOT is_active) AS Inactive
            FROM medical_centers
            """;

        return await connection.QuerySingleAsync<MedicalCenterSummaryResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
