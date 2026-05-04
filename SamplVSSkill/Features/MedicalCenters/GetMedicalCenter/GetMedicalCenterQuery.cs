using Dapper;
using SamplVSSkill.Domain.Enums;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.MedicalCenters.GetMedicalCenter;

// ── Response ────────────────────────────────────────────────────
public record GetMedicalCenterResponse(
    Guid Id,
    string Name,
    MedicalCenterType? Type,
    string? Address,
    string? Phone,
    bool IsActive,
    double? Latitude,
    double? Longitude);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class GetMedicalCenterQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public GetMedicalCenterQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<GetMedicalCenterResponse?> HandleAsync(Guid id, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id       AS Id,
                   name     AS Name,
                   type     AS Type,
                   address  AS Address,
                   phone    AS Phone,
                   is_active AS IsActive,
                   latitude  AS Latitude,
                   longitude AS Longitude
            FROM medical_centers
            WHERE id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<GetMedicalCenterResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
