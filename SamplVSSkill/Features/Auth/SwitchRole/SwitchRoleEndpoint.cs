using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Infrastructure.Middleware;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.SwitchRole;

public static class SwitchRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/switch-role", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<SwitchRoleCommand>>()
           .WithTags("Auth")
           .WithName("SwitchRole")
           .Produces<SwitchRoleResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .Produces(StatusCodes.Status404NotFound)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        SwitchRoleCommand command,
        SwitchRoleCommandHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(command, userId, ct);
    }
}
