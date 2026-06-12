using System.Security.Claims;
using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Auth.PagedUsers;

public static class PagedUsersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/auth/users/paged", Handle)
           .WithTags("Users")
           .WithName("PagedUsers")
           .Produces<PaginatedResult<PagedUserItem>>()
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        [AsParameters] PagedUsersParams queryParams,
        PagedUsersQueryHandler handler,
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

        var result = await handler.HandleAsync(queryParams, currentUserId, isSuperAdmin, ct);
        return Results.Ok(result);
    }
}
