using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SamplVSSkill.Features.Auth.UpdateUser;

public static class UpdateUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/auth/users/{id}", Handle)
           .DisableAntiforgery()
           .WithTags("Users")
           .WithName("UpdateUser")
           .Produces<UpdateUserResponse>()
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        string id,
        [FromForm] UpdateUserCommand command,
        UpdateUserCommandHandler handler,
        CancellationToken ct)
    {
        var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Results.Unauthorized();
        }

        var isSuperAdmin = principal.IsInRole("SuperAdmin");
        var isAdmin = principal.IsInRole("Admin");

        // Permite a un SuperAdmin, Admin o al propio usuario actualizar el perfil.
        if (!isSuperAdmin && !isAdmin && currentUserId != id)
        {
            return Results.Forbid();
        }

        return await handler.HandleAsync(id, command, currentUserId, isSuperAdmin, ct);
    }
}
