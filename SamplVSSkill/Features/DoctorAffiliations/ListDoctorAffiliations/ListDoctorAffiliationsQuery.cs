using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.DoctorAffiliations.ListDoctorAffiliations;

// Params allow filtering by DoctorId or CenterId
public record ListDoctorAffiliationsParams(Guid? DoctorId, Guid? CenterId);

public record DoctorAffiliationItem(
    int Id,
    Guid DoctorId,
    string DoctorName,
    Guid CenterId,
    string CenterName,
    string? OfficeNumber,
    string? WorkSchedule,
    DateTime CreatedAt);

public class ListDoctorAffiliationsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListDoctorAffiliationsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<DoctorAffiliationItem>> HandleAsync(
        ListDoctorAffiliationsParams p, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        var conditions = new List<string>();

        if (p.DoctorId.HasValue)
        {
            conditions.Add("da.doctor_id = @DoctorId");
            dp.Add("DoctorId", p.DoctorId.Value);
        }

        if (p.CenterId.HasValue)
        {
            conditions.Add("da.center_id = @CenterId");
            dp.Add("CenterId", p.CenterId.Value);
        }

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        var sql = $"""
            SELECT da.id             AS Id,
                   da.doctor_id      AS DoctorId,
                   CONCAT(d.name, ' ', d.last_name) AS DoctorName,
                   da.center_id      AS CenterId,
                   mc.name           AS CenterName,
                   da.office_number  AS OfficeNumber,
                   da.work_schedule  AS WorkSchedule,
                   da.created_at     AS CreatedAt
            FROM doctor_affiliations da
            JOIN doctors d ON da.doctor_id = d.id
            JOIN medical_centers mc ON da.center_id = mc.id
            {whereClause}
            ORDER BY mc.name, d.name
            """;

        return await connection.QueryAsync<DoctorAffiliationItem>(
            new CommandDefinition(sql, dp, cancellationToken: ct));
    }
}
