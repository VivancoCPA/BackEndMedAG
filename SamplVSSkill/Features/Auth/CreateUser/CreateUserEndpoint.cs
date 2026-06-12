using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.CreateUser;

public static class CreateUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/users", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<CreateUserCommand>>()
           .WithTags("Users")
           .WithName("CreateUser")
           .Produces<CreateUserResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status409Conflict)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        [FromForm] CreateUserCommand command,
        CreateUserCommandHandler handler,
        CancellationToken ct)
    {
        var creatorUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(creatorUserId))
        {
            return Results.Unauthorized();
        }

        var isSuperAdmin = principal.IsInRole("SuperAdmin");
        var isAdmin = principal.IsInRole("Admin");

        if (!isSuperAdmin && !isAdmin)
        {
            return Results.Forbid();
        }

        return await handler.HandleAsync(command, creatorUserId, isAdmin, ct);
    }
}
