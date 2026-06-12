using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Infrastructure.Middleware;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.AddUserScope;

public static class AddUserScopeEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/users/{adminId}/scope/{userId}", Handle)
           .AddEndpointFilter<ValidationFilter<AddUserScopeCommand>>()
           .WithTags("Users")
           .WithName("AddUserScope")
           .Produces<AddUserScopeResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status409Conflict)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        string adminId,
        string userId,
        AddUserScopeCommandHandler handler,
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

        // Un Admin estándar solo puede agregar usuarios a su PROPIO scope.
        // Un SuperAdmin puede agregar usuarios al scope de CUALQUIER Admin.
        if (!isSuperAdmin && currentUserId != adminId)
        {
            return Results.Forbid();
        }

        var command = new AddUserScopeCommand(adminId, userId);
        return await handler.HandleAsync(command, ct);
    }
}
