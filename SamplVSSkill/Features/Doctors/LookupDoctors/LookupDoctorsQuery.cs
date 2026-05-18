using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.LookupDoctors;

public class LookupDoctorsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public LookupDoctorsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<LookupItemGuid>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id   AS Id,
                   CONCAT(name, ' ', last_name) AS Name
            FROM doctors
            WHERE is_active = true
            ORDER BY name, last_name
            """;

        return await connection.QueryAsync<LookupItemGuid>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
