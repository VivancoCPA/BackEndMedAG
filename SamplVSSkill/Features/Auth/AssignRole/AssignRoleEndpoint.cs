using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.AssignRole;

public static class AssignRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/users/{userId}/roles", Handle)
           .AddEndpointFilter<ValidationFilter<AssignRoleCommand>>()
           .WithTags("Users")
           .WithName("AssignRole")
           .Produces<AssignRoleResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status409Conflict)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        AssignRoleCommand command,
        AssignRoleCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, command, ct);
}
