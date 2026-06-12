using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.ListUnscopedUsers;

public static class ListUnscopedUsersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users/unscoped", Handle)
           .WithTags("Users")
           .WithName("ListUnscopedUsers")
           .Produces<IEnumerable<ListUnscopedUsersResponse>>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        ListUnscopedUsersQueryHandler handler,
        CancellationToken ct)
    {
        var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Results.Unauthorized();
        }

        //var isSuperAdmin = principal.IsInRole("SuperAdmin");
        var isAdmin = principal.IsInRole("Admin");

        if (!isAdmin)//
        {
            return Results.Forbid();
        }

        var results = await handler.HandleAsync(ct);
        return Results.Ok(results);
    }
}
