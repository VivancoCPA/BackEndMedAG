using Dapper;
using SamplVSSkill.Domain.Enums;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.ListMedicalCenters;

// ── Response ────────────────────────────────────────────────────
public record ListMedicalCentersResponse(
    Guid Id,
    string Name,
    MedicalCenterType? Type,
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
            SELECT id         AS Id,
                   name       AS Name,
                   type       AS Type,
                   address    AS Address,
                   phone      AS Phone,
                   is_active  AS IsActive,
                   latitude   AS Latitude,
                   longitude  AS Longitude,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM medical_centers
            ORDER BY name
            """;

        return await connection.QueryAsync<ListMedicalCentersResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
