namespace SamplVSSkill.Features.Auth.RemoveClaim;

public static class RemoveClaimEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        // Query string: ?claimType=X&claimValue=Y  — DELETE cannot have a request body
        app.MapDelete("/api/users/{userId}/claims", Handle)
           .WithTags("Users")
           .WithName("RemoveClaim")
           .Produces<RemoveClaimResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        [AsParameters] RemoveClaimParams claimParams,
        RemoveClaimCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, claimParams, ct);
}
