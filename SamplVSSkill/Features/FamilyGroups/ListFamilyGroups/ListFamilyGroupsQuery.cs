using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyGroups.ListFamilyGroups;

// ── Response ────────────────────────────────────────────────────
public record ListFamilyGroupsResponse(
    Guid Id, string Name, string? UserId, string? OwnerName,
    string? PhotoUrl, bool IsActive, DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class ListFamilyGroupsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;
    public ListFamilyGroupsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListFamilyGroupsResponse>> HandleAsync(CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT fg.id           AS Id,
                   fg.name         AS Name,
                   fg.user_id      AS UserId,
                   (u."Name" || ' ' || u."LastName") AS OwnerName,
                   fg.photo_url    AS PhotoUrl,
                   fg.is_active    AS IsActive,
                   fg.created_at   AS CreatedAt
            FROM family_groups fg
            LEFT JOIN "AspNetUsers" u ON fg.user_id = u."Id"
            ORDER BY fg.name
            """;

        return await connection.QueryAsync<ListFamilyGroupsResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
