using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.ToggleUserStatus;

public static class ToggleUserStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/users/{userId}/toggle-status", Handle)
           .WithTags("Users")
           .WithName("ToggleUserStatus")
           .Produces<ToggleUserStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        string userId,
        ToggleUserStatusCommandHandler handler,
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

        return await handler.HandleAsync(userId, currentUserId, isSuperAdmin, ct);
    }
}
