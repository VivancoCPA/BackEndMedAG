using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.ListUserScopes;

public static class ListUserScopesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users/{adminId}/scopes", Handle)
           .WithTags("Users")
           .WithName("ListUserScopes")
           .Produces<IEnumerable<ListUserScopesResponse>>()
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        string adminId,
        ListUserScopesQueryHandler handler,
        CancellationToken ct)
    {
        var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Results.Unauthorized();
        }

        var isSuperAdmin = principal.IsInRole("SuperAdmin");
        var isAdmin = principal.IsInRole("Admin");

        if (!isSuperAdmin && !isAdmin)
        {
            return Results.Forbid();
        }

        // Un Admin estándar solo puede listar su PROPIO scope.
        // Un SuperAdmin puede listar el scope de CUALQUIER Admin.
        if (!isSuperAdmin && currentUserId != adminId)
        {
            return Results.Forbid();
        }

        var results = await handler.HandleAsync(adminId, ct);
        return Results.Ok(results);
    }
}
