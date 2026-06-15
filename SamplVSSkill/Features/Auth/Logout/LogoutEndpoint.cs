using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace SamplVSSkill.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/logout", Handle)
           .DisableAntiforgery()
           .WithTags("Auth")
           .WithName("Logout")
           .Produces<LogoutResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status404NotFound)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        LogoutCommandHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(userId, ct);
    }
}
