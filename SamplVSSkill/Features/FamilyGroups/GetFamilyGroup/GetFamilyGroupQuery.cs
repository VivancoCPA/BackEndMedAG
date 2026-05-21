using Dapper;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.FamilyGroups.GetFamilyGroup;

// ── Response ────────────────────────────────────────────────────
public record GetFamilyGroupResponse(
    Guid Id, string Name, string? UserId, string? OwnerName,
    string? PhotoUrl, bool IsActive, DateTime CreatedAt);

// ── Query Handler (Dapper) ──────────────────────────────────────
public class GetFamilyGroupQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;
    public GetFamilyGroupQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IResult> HandleAsync(Guid id, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT fg.id           AS Id,
                   fg.name         AS Name,
                   fg.user_id      AS UserId,
                   (u."name" || ' ' || u."last_name") AS OwnerName,
                   fg.photo_url    AS PhotoUrl,
                   fg.is_active    AS IsActive,
                   fg.created_at   AS CreatedAt
            FROM family_groups fg
            LEFT JOIN "AspNetUsers" u ON fg.user_id = u."Id"
            WHERE fg.id = @Id
            """;

        var item = await connection.QueryFirstOrDefaultAsync<GetFamilyGroupResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        return item is null
            ? Results.NotFound($"Grupo familiar '{id}' no encontrado.")
            : Results.Ok(item);
    }
}
