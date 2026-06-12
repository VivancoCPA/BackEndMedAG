using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.ListUnscopedUsers;

public record ListUnscopedUsersResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    string FullName);

public class ListUnscopedUsersQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListUnscopedUsersQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListUnscopedUsersResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT u."Id"             AS Id,
                   u."Email"          AS Email,
                   u."Name"           AS Name,
                   u."LastName"       AS LastName,
                   CONCAT(u."Name", ' ', u."LastName") AS FullName
            FROM "AspNetUsers" u
            WHERE u."Id" NOT IN (SELECT user_id FROM user_scope)
            ORDER BY u."Name", u."LastName"
            """;

        return await connection.QueryAsync<ListUnscopedUsersResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
