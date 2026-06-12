using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.RemoveUserScope;

public static class RemoveUserScopeEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/users/{adminId}/scope/{userId}", Handle)
           .WithTags("Users")
           .WithName("RemoveUserScope")
           .Produces<RemoveUserScopeResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        string adminId,
        string userId,
        RemoveUserScopeCommandHandler handler,
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

        // Un Admin estándar solo puede remover usuarios de su PROPIO scope.
        // Un SuperAdmin puede remover usuarios del scope de CUALQUIER Admin.
        if (!isSuperAdmin && currentUserId != adminId)
        {
            return Results.Forbid();
        }

        var command = new RemoveUserScopeCommand(adminId, userId);
        return await handler.HandleAsync(command, ct);
    }
}
