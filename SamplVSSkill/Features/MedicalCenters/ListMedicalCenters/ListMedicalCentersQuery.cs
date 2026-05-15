using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.ListMedicalCenters;

// ── Response ────────────────────────────────────────────────────
public record ListMedicalCentersResponse(
    Guid Id,
    string Name,
    int? TypeId,
    string? TypeName,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListMedicalCentersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListMedicalCentersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListMedicalCentersResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT mc.id         AS Id,
                   mc.name       AS Name,
                   mc.type_id    AS TypeId,
                   ct.name       AS TypeName,
                   mc.address    AS Address,
                   mc.phone      AS Phone,
                   mc.is_active  AS IsActive,
                   mc.latitude   AS Latitude,
                   mc.longitude  AS Longitude,
                   mc.created_at AS CreatedAt,
                   mc.updated_at AS UpdatedAt
            FROM medical_centers mc
            LEFT JOIN centers_type ct ON ct.id = mc.type_id
            ORDER BY mc.name
            """;

        return await connection.QueryAsync<ListMedicalCentersResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
