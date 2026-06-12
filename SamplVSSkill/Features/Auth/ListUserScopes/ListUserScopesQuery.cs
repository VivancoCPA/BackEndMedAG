using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.ListUserScopes;

public record ListUserScopesResponse(
    int Id,
    string UserIdAdmin,
    string UserId,
    string UserEmail,
    string UserFullName);

public class ListUserScopesQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListUserScopesQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListUserScopesResponse>> HandleAsync(
        string adminId, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT us.id              AS Id,
                   us.user_id_admin   AS UserIdAdmin,
                   us.user_id         AS UserId,
                   u."Email"          AS UserEmail,
                   CONCAT(u."Name", ' ', u."LastName") AS UserFullName
            FROM user_scope us
            INNER JOIN "AspNetUsers" u ON us.user_id = u."Id"
            WHERE us.user_id_admin = @AdminId
            ORDER BY u."Name", u."LastName"
            """;

        return await connection.QueryAsync<ListUserScopesResponse>(
            new CommandDefinition(sql, new { AdminId = adminId }, cancellationToken: ct));
    }
}
