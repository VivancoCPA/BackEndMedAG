using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.AssignClaim;

public static class AssignClaimEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/users/{userId}/claims", Handle)
           .AddEndpointFilter<ValidationFilter<AssignClaimCommand>>()
           .WithTags("Users")
           .WithName("AssignClaim")
           .Produces<AssignClaimResponse>(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status409Conflict)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        AssignClaimCommand command,
        AssignClaimCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, command, ct);
}
